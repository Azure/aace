"""
The flask application package.
"""

from flask import Flask
from Agent.Mgmt.ControlPlane import ControlPlane
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy import create_engine
import urllib
from sqlalchemy.orm import sessionmaker
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential
from azure.storage.blob import BlobServiceClient


app = Flask(__name__)
app.config.from_object('config')

Base = declarative_base()

credential = DefaultAzureCredential()
key_vault_client = SecretClient(vault_url=app.config['KEY_VAULT_URI'], credential=credential)


odbc_connection_string = key_vault_client.get_secret(app.config['ODBC_CONNECTION_STRING_SECRET_NAME'])
params = urllib.parse.quote_plus(odbc_connection_string.value)

engine = create_engine("mssql+pyodbc:///?odbc_connect=%s" % params)

Session = sessionmaker(bind=engine)

controlPlane = ControlPlane(app.config['CONTROL_PLANE_URL'], app.config['AGENT_ID'], app.config['AGENT_KEY'])

import Agent.views
