import numpy as np
import fastai
from fastai.vision import *
from fastai.callbacks.hooks import *

import os, glob, argparse, time, random, math

from azureml.core import Workspace, Run, Dataset

parser = argparse.ArgumentParser()
parser.add_argument('--data_folder', type=str, dest='data_folder')
parser.add_argument('--org_size', type=int, dest='org_size', default=768)
parser.add_argument('--train_folder', type=str, dest='train_folder', default='train_v2')
parser.add_argument('--train_sgmtfile', type=str, dest='train_sgmtfile', default='train_ship_segmentations_v2.csv')
parser.add_argument('--class_folder', type=str, dest='class_folder', default='class')
parser.add_argument('--img_size', type=int, dest='img_size', default=256)
parser.add_argument('--min_area', type=int, dest='min_area', default=99)
parser.add_argument('--sgmtimg_folder', type=str, dest='sgmtimg_folder', default='256-filter99')
parser.add_argument('--sgmtlabel_folder', type=str, dest='sgmtlabel_folder', default='256-label')
args = parser.parse_args()

run = Run.get_context()

data_folder = args.data_folder
print('Data folder: ', data_folder)

train_folder = os.path.join(data_folder, args.train_folder)
SEGMENTATION = os.path.join(data_folder, args.train_sgmtfile)

# Clean images
print('Searching the broken images.............')
brokenfiles = []
for fpath in glob.glob(os.path.join(train_folder, '*.jpg')):
    try:
        img = open_image(fpath)
    except:
        fn = os.path.basename(fpath)
        print(fn, ' is broken')
        brokenfiles.append(fn)
print(len(brokenfiles), ' images are broken')

print('Moving broken images.........')
broken_folder = os.path.join(train_folder, 'broken')
os.makedirs(broken_folder, exist_ok=True)
for fn in brokenfiles:
    orig_name = os.path.join(train_folder, fn)
    new_name = os.path.join(broken_folder, fn)
    os.rename(orig_name, new_name)
	
# Divide images into Ship and NoShip
print('Split images to ship & no-ship folder .........')
df_masks = pd.read_csv(SEGMENTATION, index_col='ImageId')

class_folder = os.path.join(train_folder, args.class_folder)

ship_folder = os.path.join(class_folder, 'ship')
noship_folder = os.path.join(class_folder, 'no-ship')

for fpath in glob.glob(os.path.join(train_folder, '*.jpg')):
	fn = os.path.basename(fpath)
	if isinstance(df_masks.loc[fn,'EncodedPixels'], str):
		tpath = os.path.join(ship_folder, fn)
	else:
		tpath = os.path.join(noship_folder, fn)

	os.rename(fpath, tpath)

print('Generating lable files............')
sz_enc = [args.org_size, args.org_size]

def enc2mask(masks, shape = sz_enc):
    img = np.zeros(shape[0]*shape[1], dtype=np.uint8)

    if(type(masks) == float): return img.reshape(shape)
    if(type(masks) == str): masks = [masks]
    for mask in masks:
        s = mask.split()
        for i in range(len(s)//2):
            start = int(s[2*i]) - 1
            length = int(s[2*i+1])
            img[start:start+length] = 1
    return img.reshape(shape).T

label_folder = os.path.join(train_folder, 'label')

for fpath in glob.glob(os.path.join(ship_folder, '*.jpg')):
	fn = os.path.basename(fpath)
	labelpath = os.path.join(label_folder, Path(fn).stem + '.png')

	mask = enc2mask(df_masks.loc[fn,'EncodedPixels'])
	maskimg = PIL.Image.fromarray(mask)
	maskimg.save(labelpath)


def SummaryLabelArea(label_root):
    min_area = 1000000
    area_hist = np.zeros(20, int)
    for fpath in glob.glob(os.path.join(label_root, '*.png')):
        mask = open_mask(fpath)
        area = mask.data.sum()
        area_hist[int(math.log2(area))] += 1
        if area < min_area: min_area = area
    
    print('Min area is ', min_area)
    print(area_hist / np.sum(area_hist))
    
    return min_area, area_hist

SummaryLabelArea(label_folder);

print('Resizing images and labels .........')
def ResizeTrainLabel(train_root, label_root, dest_train_root, dest_label_root, size, min_area = 0):
           
    for fpathstr in glob.glob(os.path.join(train_root, '*.jpg')):
        fpath = Path(fpathstr)
        lpath = os.path.join(label_root, fpath.stem + '.png')
    
        mask = open_mask(lpath)
        mask = mask.resize(size)
        
        if mask.data.sum() > min_area:
            dest_lpath = os.path.join(dest_label_root, fpath.stem + '.png')
            mask.save(dest_lpath)
        
            img = open_image(fpath)
            img = img.resize(size)

            dest_fpath = os.path.join(dest_train_root, fpath.stem + '.jpg')
            img.save(dest_fpath)

sgmtimg_folder = os.path.join(train_folder, args.sgmtimg_folder)
sgmtlabel_folder = os.path.join(train_folder, args.sgmtlabel_folder)

ResizeTrainLabel(ship_folder, label_folder, sgmtimg_folder, sgmtlabel_folder, args.img_size, args.min_area)
