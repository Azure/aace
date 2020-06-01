import numpy as np
import fastai
from fastai.vision import *
from fastai.callbacks.hooks import *
from fastai.callbacks.mem import PeakMemMetric
from fastai.distributed import *

import os, argparse, time, random
from azureml.core import Workspace, Run, Dataset

from azureml_adapter import set_environment_variables_for_nccl_backend, get_local_rank, get_global_size, get_local_size

parser = argparse.ArgumentParser()
parser.add_argument('--data_folder', type=str, dest='data_folder', default='')
parser.add_argument('--img_size', type=int, dest='img_size', default=256)
parser.add_argument('--batch_size', type=int, dest='banch_size', default=64)
parser.add_argument('--num_epochs', type=int, dest='num_epochs', default=12)
parser.add_argument('--start_learning_rate', type=float, dest='start_learning_rate', default=0.001)
parser.add_argument('--end_learning_rate', type=float, dest='end_learning_rate', default=0.01)
parser.add_argument('--pct_start', type=float, dest='pct_start', default=0.9)
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

data = ImageDataBunch.from_folder(data_folder, train=".", valid_pct=0.2,
       ds_tfms=get_transforms(), size=sz, bs = bs, num_workers=8).normalize(imagenet_stats)
	   
learn = cnn_learner(data, models.resnet34, metrics=dice).to_distributed(local_rank)
learn.fit_one_cycle(args.num_epochs, slice(args.start_learning_rate,args.end_learning_rate))

#learn.unfreeze()
#learn.fit_one_cycle(5, slice(1e-5), pct_start=0.8)

result = learn.validate()
run.log('Worker #{:} loss'.format(rank), np.float(result[0]))
run.log('Worker #{:} dice'.format(rank), np.float(result[1]))

os.chdir(work_folder)
if rank == 0:
	run.log('loss', np.float(result[0]))
	run.log('dice', np.float(result[1]))

	#filename = 'outputs/classification.pkl'
	#learn.export(outputs/)