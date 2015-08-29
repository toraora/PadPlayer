import urllib, urllib2, time, datetime, sys
#from com.android.monkeyrunner import MonkeyRunner, MonkeyDevice
sys.path.append("C:\\Python27\\Lib\\site-packages")
import simplejson as json

WIDTH = 800
HEIGHT = 650

ROWS = 5
COLS = 6

OFFSET_W = 0 + WIDTH / COLS / 2
OFFSET_H = 570 + HEIGHT / ROWS / 2

def get_loc(row, col):
  y = HEIGHT * row / ROWS
  x = WIDTH * col / COLS
  return (x + OFFSET_W, y + OFFSET_H)

def play(device):
  # take screenshot and send to api endpoint
#  result = device.takeSnapshot()
  filename = "C:\\Users\\Nathan\\Documents\\PadImages\\PAD.png"
#  result.writeToFile(filename, "png")
  apiCall = 'http://localhost:5000/solve?rows=%i&cols=%i&width=%i&height=%i&w_off=%i&h_off=%i' %(ROWS, COLS, WIDTH, HEIGHT, OFFSET_W, OFFSET_H)
  print apiCall
  print urllib2.urlopen(apiCall, urllib.urlencode({'path' : filename}))
  data = json.loads(urllib2.urlopen(apiCall, urllib.urlencode({'path' : filename})).read())
  print data
  # data = json.load("""{"Start":{"Item1":4,"Item2":4},"Score":460986.1875,"Depth":26,"Current":{"Item1":3,"Item2":5},"Length":24,"Actions":[[0,-1],[-1,0],[0,1],[-1,0],[0,1],[1,0],[1,0],[0,-1],[-1,0],[-1,0],[-1,0],[-1,0],[0,-1],[1,0],[0,1],[0,1],[-1,0],[0,-1],[0,-1],[1,0],[0,1],[0,1],[1,0],[1,0]]}""")
  # data = {}
  # data["Actions"] = [[0,-1],[-1,0],[0,1],[-1,0],[0,1],[1,0],[1,0],[0,-1],[-1,0],[-1,0],[-1,0],[-1,0],[0,-1],[1,0],[0,1],[0,1],[-1,0],[0,-1],[0,-1],[1,0],[0,1],[0,1],[1,0],[1,0]]
  # data["Start"] = [4, 4]
  x, y = data["Start"]["Item2"], data["Start"]["Item1"]
  cur_loc = get_loc(y, x)
  #device.touch(cur_loc[0], cur_loc[1], MonkeyDevice.DOWN)
  for action in data["Actions"]:
    for _ in range(5):
      time.sleep(0.001)
      y = 0. + y + action[0] / 5.
      x = 0. + x + action[1] / 5.
      cur_loc = get_loc(y, x)
#      device.touch(cur_loc[0], cur_loc[1], MonkeyDevice.MOVE)
#  device.touch(cur_loc[0], cur_loc[1], MonkeyDevice.UP)

#device = MonkeyRunner.waitForConnection()
play(1)
