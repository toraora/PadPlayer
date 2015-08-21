import urllib2, time
from com.android.monkeyrunner import MonkeyRunner, MonkeyDevice

WIDTH = 1080
HEIGHT = 920

ROWS = 5
COLS = 6

OFFSET_W = 0 + WIDTH / COLS / 2
OFFSET_H = 900 + HEIGHT / ROWS / 2

def get_loc(row, col):
  y = HEIGHT * row / ROWS
  x = WIDTH * col / COLS
  return (x + OFFSET_W, y + OFFSET_H)

def play(device):
  # take screenshot and send to api endpoint
  # data = json.load(urllib2.urlopen('http://localhost:5000'))
  # data = json.load("""{"Start":{"Item1":4,"Item2":4},"Score":460986.1875,"Depth":26,"Current":{"Item1":3,"Item2":5},"Length":24,"Actions":[[0,-1],[-1,0],[0,1],[-1,0],[0,1],[1,0],[1,0],[0,-1],[-1,0],[-1,0],[-1,0],[-1,0],[0,-1],[1,0],[0,1],[0,1],[-1,0],[0,-1],[0,-1],[1,0],[0,1],[0,1],[1,0],[1,0]]}""")
  data = {}
  data["Actions"] = [[0,-1],[-1,0],[0,1],[-1,0],[0,1],[1,0],[1,0],[0,-1],[-1,0],[-1,0],[-1,0],[-1,0],[0,-1],[1,0],[0,1],[0,1],[-1,0],[0,-1],[0,-1],[1,0],[0,1],[0,1],[1,0],[1,0]]
  data["Start"] = [4, 4]
  x, y = data["Start"][0], data["Start"][1]
  cur_loc = get_loc(y, x)
  device.touch(cur_loc[0], cur_loc[1], MonkeyDevice.DOWN)
  for action in data["Actions"]:
    for _ in range(10):
      time.sleep(0.005)
      y = 0. + y + action[0] * 0.1
      x = 0. + x + action[1] * 0.1
      cur_loc = get_loc(y, x)
      device.touch(cur_loc[0], cur_loc[1], MonkeyDevice.MOVE)
  device.touch(cur_loc[0], cur_loc[1], MonkeyDevice.UP)

device = MonkeyRunner.waitForConnection()
play(device)
