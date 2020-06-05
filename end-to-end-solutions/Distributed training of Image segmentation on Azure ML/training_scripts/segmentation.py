import numpy as np
import fastai
from fastai.vision import *
from fastai.callbacks.hooks import *
from fastai.callbacks.mem import PeakMemMetric
from fastai.distributed import *

import os, argparse, time, random
from azureml.core import Workspace, Run, Dataset

from azureml_adapter import set_environment_variables_for_nccl_backend, get_local_rank, get_global_size, get_local_size

def dice_loss(input, target):
    #input = torch.sigmoid(input)
    smooth = 1.0

    iflat = input.flatten()
    tflat = target.flatten()
    intersection = (iflat * tflat).sum()
    
    return ((2.0 * intersection + smooth) / (iflat.sum() + tflat.sum() + smooth))

class FocalLoss(nn.Module):
    def __init__(self, gamma):
        super().__init__()
        self.gamma = gamma
        
    def forward(self, input, target):
        if not (target.size() == input.size()):
            raise ValueError("Target size ({}) must be the same as input size ({})"
                             .format(target.size(), input.size()))

        max_val = (-input).clamp(min=0)
        loss = input - input * target + max_val + \
            ((-max_val).exp() + (-input - max_val).exp()).log()

        invprobs = F.logsigmoid(-input * (target * 2.0 - 1.0))
        loss = (invprobs * self.gamma).exp() * loss
        
        return loss.mean()

class MixedLoss(nn.Module):
    def __init__(self, alpha, gamma):
        super().__init__()
        self.alpha = alpha
        self.focal = FocalLoss(gamma)
        
    def forward(self, input, target):
        input = F.softmax(input, dim=1)[:,1:,:,:]
        input2 = torch.log((input+1e-7)/(1-input+1e-7))

        loss = self.alpha*self.focal(input2, target) - torch.log(dice_loss(input, target))
        return loss

parser = argparse.ArgumentParser()
parser.add_argument('--data_folder', type=str, dest='data_folder', default='')
parser.add_argument('--label_folder', type=str, dest='label_folder', default='256-label')
parser.add_argument('--img_folder', type=str, dest='img_folder', default='256-filter')
parser.add_argument('--img_size', type=int, dest='img_size', default=256)
parser.add_argument('--batch_size', type=int, dest='banch_size', default=16)
parser.add_argument('--num_epochs', type=int, dest='num_epochs', default=12)
parser.add_argument('--start_learning_rate', type=float, dest='start_learning_rate', default=0.000001)
parser.add_argument('--end_learning_rate', type=float, dest='end_learning_rate', default=0.001)
args = parser.parse_args()

local_rank = -1
local_rank = get_local_rank()
global_size = get_global_size()
local_size = get_local_size()	

# TODO use logger	
print('local_rank = {}'.format(local_rank))
print('global_size = {}'.format(global_size))
print('local_size = {}'.format(local_size))

set_environment_variables_for_nccl_backend(local_size == global_size)
torch.cuda.set_device(local_rank)
torch.distributed.init_process_group(backend='nccl', init_method='env://')
rank = int(os.environ['RANK'])

data_folder = args.data_folder
sz = args.img_size
bs = args.banch_size
print('Data folder:', data_folder)

run = Run.get_context()
work_folder = os.getcwd()
print('Work directory: ', work_folder)

label_path = Path(os.path.join(data_folder, args.label_folder))
get_y_fn = lambda x: label_path/f'{x.stem}.png'
tfms = get_transforms(max_rotate = 10, max_lighting = 0.05, max_warp = 0.2, flip_vert = True,
                      p_affine = 1., p_lighting = 1)

img_path = os.path.join(data_folder, args.img_folder)
data = (SegmentationItemList.from_folder(img_path)
        .split_by_rand_pct(0.2)
        .label_from_func(get_y_fn, classes=['Background','Ship'])
        .transform(tfms, size=sz, tfm_y=True)
        .databunch(path=Path('.'), bs=bs, num_workers=0)
        .normalize(imagenet_stats))

learn = unet_learner(data, models.resnet34, loss_func=MixedLoss(10.0,2.0), metrics=dice, wd=1e-7).to_distributed(local_rank)
learn.fit_one_cycle(args.num_epochs, slice(args.start_learning_rate,args.end_learning_rate))
 
#learn.unfreeze()
#learn.fit_one_cycle(args.num_epochs, slice(args.start_learning_rate,args.end_learning_rate))

result = learn.validate()
run.log('Worker #{:} loss'.format(rank), np.float(result[0]))
run.log('Worker #{:} dice'.format(rank), np.float(result[1]))

if rank == 0:
	run.log('loss', np.float(result[0]))
	run.log('dice', np.float(result[1]))
	
	os.chdir(work_folder)
	filename = 'outputs/segmentation.pkl'
	learn.export(filename)