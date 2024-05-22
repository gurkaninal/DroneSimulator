import sys
import time
from datetime import datetime

commands = [1, 1, 1, 0, 1, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2, 0, 0, 0]

i = 0
while True:
    print(f"{commands[i % len(commands)]}")
    sys.stdout.flush()
    time.sleep(1)  # Wait for 2 seconds
    i += 1