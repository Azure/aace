


class LunaServerException(Exception):
    http_status_code = 500
    message = ''

    def __init__(self, message):
        self.message = message

class LunaUserException(Exception):
    http_status_code = 500
    message = ''

    def __init__(self, status_code, message):
        self.http_status_code = status_code
        self.message = message