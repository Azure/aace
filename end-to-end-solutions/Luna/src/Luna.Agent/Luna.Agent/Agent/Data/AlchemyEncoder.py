from sqlalchemy.ext.declarative import DeclarativeMeta
from flask.json import JSONEncoder
import json
import datetime

class AlchemyEncoder(JSONEncoder):

    def default(self, obj):
        if isinstance(obj.__class__, DeclarativeMeta):
            # an SQLAlchemy class
            fields = {}
            for field in [x for x in dir(obj) if not x.startswith('_') and x != 'metadata']:
                data = obj.__getattribute__(field)
                if not callable(data):
                    try:
                        json.dumps(data, cls=AlchemyEncoder) # this will fail on non-encodable values, like other classes
                        fields[field] = data
                    except TypeError:
                        fields[field] = None
            # a json-encodable dict
            return fields
        elif isinstance(obj, datetime.datetime):
            return obj.isoformat()
        return json.JSONEncoder.default(self, obj)
