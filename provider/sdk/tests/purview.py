from azure.identity import ClientSecretCredential

import requests

TENANT_ID = "72f988bf-86f1-41af-91ab-2d7cd011db47"
SCOPE = "https://purview.azure.net/.default"
CLIENT_ID = "7de0a692-d821-4033-9bd6-348d37fcc79c"
CLIENT_SECRET = "VtN7Q~ARoAbNrQ22kWgOW_T13SNrFKQFZ2LLY"
HOST_URL = "https://xiwutest.purview.azure.com/catalog"

credential = ClientSecretCredential(TENANT_ID, CLIENT_ID, CLIENT_SECRET)

accessToken = credential.get_token(SCOPE)

entities = {
  "referredEntities": {},
  "entity": {
    "typeName": "azure_storage_account",
    "attributes": {
      "owner": "ExampleOwner",
      "modifiedTime": 0,
      "createTime": 0,
      "qualifiedName": "https://exampleaccount.core.windows.net",
      "name": "ExampleStorageAccount",
      "description": None,
      "publicAccessLevel": None
    },
    "contacts": {
      "Expert": [
        {
          "id": "30435ff9-9b96-44af-a5a9-e05c8b1ae2df",
          "info": "Example Expert Info"
        }
      ],
      "Owner": [
        {
          "id": "30435ff9-9b96-44af-a5a9-e05c8b1ae2df",
          "info": "Example Owner Info"
        }
      ]
    },
    "status": "ACTIVE",
    "createdBy": "ExampleCreator",
    "updatedBy": "ExampleUpdator",
    "version": 0
  }
}

url = "https://xiwutest.purview.azure.com/catalog/api/atlas/v2/entity/bulk"



headers = {"authorization": "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Imwzc1EtNTBjQ0g0eEJWWkxIVEd3blNSNzY4MCIsImtpZCI6Imwzc1EtNTBjQ0g0eEJWWkxIVEd3blNSNzY4MCJ9.eyJhdWQiOiI3M2MyOTQ5ZS1kYTJkLTQ1N2EtOTYwNy1mY2M2NjUxOTg5NjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNjM0MTUwODY2LCJuYmYiOjE2MzQxNTA4NjYsImV4cCI6MTYzNDE1NDc2NiwiYWNyIjoiMSIsImFpbyI6IkFWUUFxLzhUQUFBQTFGeDlHL0g2LzJqcVQzbGMxaGtoQ1d6K3dsYy9wUGFHVnV4b1ljbzA0MXZEZmIydVJERjhNTFp6eUNVMWdiYnpJN2c3NVVIa2d2UUVUVXBRUkpuTmFvaG5DazNic3VwRmJBQ3FmeGFucTlVPSIsImFtciI6WyJyc2EiLCJ3aWEiLCJtZmEiXSwiYXBwaWQiOiI2MzJkODAzYS1iMGMyLTQ5YjQtYTk0NC1lMTNjMzg0YzA0YTgiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjI1NjllNDgwLTliNWQtNDFlZC05NTlkLTJhYjMzMjIyYWI3OCIsImZhbWlseV9uYW1lIjoiV3UiLCJnaXZlbl9uYW1lIjoiWGlhb2NoZW4iLCJpbl9jb3JwIjoidHJ1ZSIsImlwYWRkciI6IjcxLjIxMi4xMDUuMTUzIiwibmFtZSI6IlhpYW9jaGVuIFd1Iiwib2lkIjoiNzkwZjE4NzctYmQ5MS00MWZhLWI0YTItOWQ2OTc0M2VlMGM0Iiwib25wcmVtX3NpZCI6IlMtMS01LTIxLTIxMjc1MjExODQtMTYwNDAxMjkyMC0xODg3OTI3NTI3LTEzMzE1NDc1IiwicHVpZCI6IjEwMDM3RkZFODAxQkEzNzciLCJyaCI6IjAuQVJvQXY0ajVjdkdHcjBHUnF5MTgwQkhiUnpxQUxXUENzTFJKcVVUaFBEaE1CS2dhQUVVLiIsInNjcCI6ImRlZmF1bHQiLCJzdWIiOiJnUm1KeU1oTU41R2NhTnJXVW15LTd0V2FyUEhMU3JNVzdWTkJDTFIzRDlZIiwidGlkIjoiNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3IiwidW5pcXVlX25hbWUiOiJ4aXd1QG1pY3Jvc29mdC5jb20iLCJ1cG4iOiJ4aXd1QG1pY3Jvc29mdC5jb20iLCJ1dGkiOiJkT2lBUnFWMGVrbWV1MEhQNWRGdkFBIiwidmVyIjoiMS4wIn0.F3yH3sOINok0TbIlrV72AZbn4YRUDK9brYltCzdNuV_pLNvGSLCPhbIrqnBOmd8_MZ2jEA89x5mOf6N7wT50cOkWhDi_4JFgbVoRsvg2-4d2z2sxcgo3Lz3O8d7a8_uKbSVrzMs3NPJMz5ia9y9c-tEhSjN5TXahWZO8jMkWzchDYLDgIKYrW56TsXyhnTSkQkahBpF1-jOXk_5K551sklseC4dUJhqKVt48QDk9HKYlrWecDCtzV7RC341MP5DdwqcVibYBIJseS1sy9kEXktNQ0BV0xLJcuwSid4fkgCcbxW3AHz7oDMzkDZ0KilxSIJ-zSkIhgr9UVrM4BgoYfQ"}
resp = requests.get(url, headers)
print(resp.json())


url = HOST_URL + "/api/atlas/v2/entity"
print(url)
headers = {"authorization": f"Bearer {accessToken}"}
resp = requests.post(url, headers, json=entities)
print(resp.json())