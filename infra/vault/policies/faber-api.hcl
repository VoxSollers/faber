# KV v2 reads required by the API; provision secrets separately.
path "secrets/data/keycloak" {
  capabilities = ["read"]
}

path "secrets/data/mail" {
  capabilities = ["read"]
}

path "secrets/data/database" {
  capabilities = ["read"]
}

path "auth/token/lookup-self" {
  capabilities = ["read"]
}

path "auth/token/renew-self" {
  capabilities = ["update"]
}
