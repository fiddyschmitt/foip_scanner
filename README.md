# foip_scanner
A port scanner written in C#

Usage:

//Scan a single host for open web interfaces (http and https)

    foip.exe -i "192.168.1.1" -p "80, 443"

//Scan a range of hosts

    foip.exe -i "192.168.1.1-192.168.1.254" -p "80, 443"

//Scan a range of hosts for a range of ports

    foip.exe -i "192.168.1.1-192.168.1.254" -p "1-8000"

//Mix single and multiple hosts

    foip.exe -i "10.0.0.1, 192.168.1.1-192.168.1.254" -p "80, 443"

//Mix single and multiple ports

    foip.exe -i "192.168.1.1-192.168.1.254" -p "80, 443, 8000-8100"
  
//Format the output of the program

    foip.exe -i "192.168.1.1 - 192.168.1.255" -p "21, 22, 80, 443, 3389" -f "{DATE}, {IP}:{PORT}, {SCHEME}://{FQDN}/"
  
  Produces this:
  
    01/11/2017 23:33:00, 192.168.1.1:80, http://magpie/

//Control the number of simultaneous connection attempts

    foip -i "192.168.1.1-192.168.255.255" -p "80, 443" -c 20

//Control the TCP connection timeout (milliseconds)

    foip -i "192.168.1.1-192.168.255.255" -p "80, 443" -t 5000

//Randomize the order of the IPs

    foip -i "192.168.1.1-192.168.255.255" -p "80, 443" -r
