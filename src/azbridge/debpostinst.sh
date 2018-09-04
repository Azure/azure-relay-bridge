echo '#!/bin/bash' > /etc/profile.d/azbridge.sh
echo 'export PATH="\$PATH:/usr/share/azbridge"' >> /etc/profile.d/azbridge.sh
echo '. /usr/share/azbridge/hostnames.sh' >> /etc/profile.d/azbridge.sh
chmod a+x /etc/profile.d/azbridge.sh
if [ ! -d "/etc/azbridge" ]; then 
   mkdir /etc/azbridge; 
fi
if [ ! -f "/etc/azbridge/azbridge_config.machine.yml" ]; then 
   mv /usr/share/azbridge/azbridge_config.machine.yml /etc/azbridge/azbridge_config.machine.yml; 
fi
/etc/profile.d/azbridge.sh