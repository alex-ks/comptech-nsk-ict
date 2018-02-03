import requests



if __name__ == "__main__":

    url = "http://tower.ict.nsc.ru"
    data = {'grant_type=password&username=UserTest&password=Test@123'}
    headers = {'Content-Type': 'application/x-www-form-urlencoded'}
    r = requests.post(url,data=data, json = headers)
    print(r.status_code)
