import sys
import time
from datetime import datetime

for i in range(20):
    current_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    print(f"{current_time}")
    sys.stdout.flush()
    time.sleep(2)  # Wait for 2 seconds