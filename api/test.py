import requests
import time

url = 'http://localhost:5000/mikuapi/changeprefix/919316515919118406'

x = requests.post(url, json = {"prefix":"$"})

print(x.text)
time.sleep(1)