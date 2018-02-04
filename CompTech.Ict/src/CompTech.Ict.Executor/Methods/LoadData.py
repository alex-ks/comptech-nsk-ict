import requests
from urllib.parse import urlencode
from urllib.request import Request, urlopen
import json
import os
import sys

if __name__ == "__main__":
    argFiles = []
    nameFiles = os.listdir(sys.argv[1]) 
    nameFiles.remove("source.py") 
    for i in range(len(nameFiles)):
        with open(os.path.join(sys.argv[1],nameFiles[i]), 'r') as f:
            arg = f.readline()
        argFiles.append((arg)) 
    dateFrom = argFiles[0]
    dateFrom = dateFrom.replace(' ','+')
    dateFinish = argFiles[1]
    dateFinish = dateFinish.replace(' ','+')
    typeId = argFiles[2]
    deviceId = argFiles[3]
    
    url = 'http://svet.akadempark.com:12200/connect/token'
    post_fields = {'grant_type': 'password', 'username': 'UserTest','password': 'Test@123'}
    request = Request(url, urlencode(post_fields).encode())
    jsonFromPost = urlopen(request).read().decode()
    ParsePostJson = json.loads(jsonFromPost)
    access_token = ParsePostJson['access_token']

    url = "http://svet.akadempark.com:12200/api/Containers?Start="+dateFrom+"&End="+dateFinish+"&TypeId="+typeId+"&DeviceId="+deviceId
    post_fields = {'Authorization': 'Bearer ' + str(access_token)}
    request = requests.get(url, headers={'Authorization': 'Bearer ' + str(access_token)})
    ParseGetJson = json.loads(request.content)

    countRecords = len(ParseGetJson)
    with open(os.path.join(sys.argv[1],str(1)+"output.txt"), 'w') as f:  
        for i in range(countRecords):
            countValue = len(ParseGetJson[i]['points'])
            for j in range(countValue - 1):
                f.write(str(ParseGetJson[i]['points'][j]['y'])+ " ")
        f.write(str(ParseGetJson[countRecords-1]['points'][countValue - 1]['y']))
    print(1)                



    
