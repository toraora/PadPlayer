import urllib, urllib2, time, datetime, sys
from com.android.monkeyrunner import MonkeyRunner, MonkeyDevice
sys.path.append("C:\\Python27\\Lib\\site-packages")
import simplejson as json

ROWS = 5
COLS = 6
WIDTH = 800
HEIGHT = 650
OFFSET_W = 0 + WIDTH / COLS / 2
OFFSET_H = 570 + HEIGHT / ROWS / 2

## lg g2
"""
WIDTH = 1080
HEIGHT = 920

ROWS = 5
COLS = 6

OFFSET_W = 0 + WIDTH / COLS / 2
OFFSET_H = 900 + HEIGHT / ROWS / 2
"""

def get_loc(row, col):
  y = HEIGHT * row / ROWS
  x = WIDTH * col / COLS
  return (x + OFFSET_W, y + OFFSET_H)

def play(device):
  # take screenshot and send to api endpoint
  result = device.takeSnapshot()
  filename = "C:\\Users\\Nathan\\Documents\\PadImages\\PAD-" + datetime.datetime.now().strftime("%d%m%Y-%H%M%S") + ".png"
  result.writeToFile(filename, "png")
  apiCall = 'http://localhost:5000/solve?rows=%i&cols=%i&width=%i&height=%i&w_off=%i&h_off=%i' %(ROWS, COLS, WIDTH, HEIGHT, OFFSET_W, OFFSET_H)
  print apiCall
  data = json.loads(urllib2.urlopen(apiCall, urllib.urlencode({'path' : filename})).read())
  # data = {}
  # data["Actions"] = [[0,-1],[-1,0],[0,1],[-1,0],[0,1],[1,0],[1,0],[0,-1],[-1,0],[-1,0],[-1,0],[-1,0],[0,-1],[1,0],[0,1],[0,1],[-1,0],[0,-1],[0,-1],[1,0],[0,1],[0,1],[1,0],[1,0]]
  # data["Start"] = [4, 4]
  x, y = data["Start"]["Item2"], data["Start"]["Item1"]
  cur_loc = get_loc(y, x)
  device.touch(cur_loc[0], cur_loc[1], MonkeyDevice.DOWN)
  for action in data["Actions"]:
    for _ in range(5):
      time.sleep(0.0015)
      y = 0. + y + action[0] / 5.
      x = 0. + x + action[1] / 5.
      cur_loc = get_loc(y, x)
      device.touch(cur_loc[0], cur_loc[1], MonkeyDevice.MOVE)
  device.touch(cur_loc[0], cur_loc[1], MonkeyDevice.UP)

device = MonkeyRunner.waitForConnection()
play(device)
