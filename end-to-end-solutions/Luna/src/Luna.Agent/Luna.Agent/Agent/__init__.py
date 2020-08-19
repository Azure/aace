"""
The flask application package.
"""

from flask import Flask
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy import create_engine
import urllib
from sqlalchemy.orm import sessionmaker
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential
from azure.storage.blob import BlobServiceClient
from Agent.Data.AlchemyEncoder import AlchemyEncoder


app = Flask(__name__)
app.config.from_object('config')
app.json_encoder = AlchemyEncoder

Base = declarative_base()

credential = DefaultAzureCredential()
key_vault_client = SecretClient(vault_url=app.config['KEY_VAULT_URI'], credential=credential)


odbc_connection_string = key_vault_client.get_secret(app.config['ODBC_CONNECTION_STRING_SECRET_NAME'])
params = urllib.parse.quote_plus(odbc_connection_string.value)

engine = create_engine("mssql+pyodbc:///?odbc_connect=%s" % params)

Session = sessionmaker(bind=engine, autoflush=False)


import Agent.views
