

sc.exe create "Consul" binPath= "d:\_backend\consul\bin\Consul.exe agent -config-dir=d:\_backend\consul\config" start= auto

sc.exe start "Consul"