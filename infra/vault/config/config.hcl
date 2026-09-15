storage "file" {
  path = "/vault/file"
}

listener "tcp" {
  address       = "0.0.0.0:8200"
  tls_disable   = true
  tls_cert_file = "/vault/certs/cert.pem"
  tls_key_file  = "/vault/certs/key.pem"
}

api_addr          = "http://127.0.0.1:8200"
default_lease_ttl = "168h"
max_lease_ttl     = "720h"

ui              = true
disable_mlock   = true
