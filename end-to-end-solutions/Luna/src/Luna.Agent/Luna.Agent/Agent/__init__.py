"""
The flask application package.
"""

from flask import Flask
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy import create_engine
import urllib, os
from sqlalchemy.orm import sessionmaker
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential
from azure.storage.blob import BlobServiceClient
from Agent.Data.AlchemyEncoder import AlchemyEncoder
from Agent.Data.KeyVaultHelper import KeyVaultHelper


app = Flask(__name__)
app.config.from_object('config')
app.json_encoder = AlchemyEncoder

Base = declarative_base()

credential = DefaultAzureCredential()
key_vault_client = SecretClient(vault_url='https://{}.vault.azure.net/'.format(os.environ['KEY_VAULT_NAME']), credential=credential)

key_vault_helper = KeyVaultHelper(key_vault_client)

odbc_connection_string = os.environ['ODBC_CONNECTION_STRING']
params = urllib.parse.quote_plus(odbc_connection_string)

engine = create_engine("mssql+pyodbc:///?odbc_connect=%s" % params)

Session = sessionmaker(bind=engine, autoflush=False)


import Agent.views
