PROJDIR := $(realpath $(CURDIR))

.PHONY: clean

clean:
	(cd $(PROJDIR)/tls-gen && git clean -xffd)

$(PROJDIR)/tls-gen/basic/result/server_rabbitmq_certificate.pem:
	cd $(PROJDIR)/tls-gen/basic && make CN=rabbitmq

certs: $(PROJDIR)/tls-gen/basic/result/server_rabbitmq_certificate.pem
