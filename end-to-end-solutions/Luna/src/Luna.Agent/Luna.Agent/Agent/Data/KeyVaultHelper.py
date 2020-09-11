from azure.keyvault.secrets import SecretClient

class KeyVaultHelper():

    _key_vault_client = None
    _secret_buffer = {}

    def __init__(self, key_vault_client):
        self._key_vault_client = key_vault_client

    def get_secret(self, secret_name, use_buffer = True):
        if use_buffer and secret_name in self._secret_buffer:
            return self._secret_buffer[secret_name]
        else:
            secret = self._key_vault_client.get_secret(secret_name).value
            # update buffer anyway
            self._secret_buffer[secret_name] = secret
            return secret

    def set_secret(self, secret_name, secret_value):
        self._secret_buffer[secret_name] = secret_value
        self._key_vault_client.set_secret(secret_name, secret_value)

    def find_secret_name_by_value(self, secret_value):
        for secret_name in self._secret_buffer:
            # if find the secret with specified value, double check with the secret in key vault
            if self._secret_buffer[secret_name] == secret_value:
                new_secret_value = self._key_vault_client.get_secret(secret_name).value
                # if the value in key vault matchs the value in buffer, return secret name
                # otherwise, return None (the user is using an old key) and update the buffer
                if new_secret_value == secret_value:
                    return secret_name
                else:
                    self._secret_buffer[secret_name] = new_secret_value
                    return None

        # if didn't find the secret with specified value in buffer, search the key vault and update the buffer
        secrets = self._key_vault_client.list_properties_of_secrets()
        for secret in secrets:
            value = self._key_vault_client.get_secret(secret.name).value
            if value == secret_value:
                self._secret_buffer[secret.name] = value
                return secret.name
            # TODO: should we update the buffer for other secrets here??

        # if still didn't find the secret with sepcified value, return None
        return None