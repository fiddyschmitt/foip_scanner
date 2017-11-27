# foip_scanner
A port scanner written in C#

Usage:

//Scan a single host for open web interfaces (http and https)

    foip.exe -i "192.168.1.1" -p "80, 443"

//Scan a range of hosts for a range of ports

    foip.exe -i "192.168.1.1-192.168.1.255" -p "1-8000"
  
//Format the output of the program

    foip.exe -i "192.168.1.1 - 192.168.1.255" -p "21, 22, 80, 443, 3389" -f "{DATE}, {IP}:{PORT}, {SCHEME}://{FQDN}/"
  
  Produces this:
  
    01/11/2017 23:33:00, 192.168.1.1:80, http://magpie/

//Order the output of the program
```
foip.exe -i "192.168.1.1-192.168.1.255" -p "1-8000" --order-by "{IP} asc, {PORT} desc"
```
    
 Will sort the results first by IP ascending, then by port descending. Sample output:
 ```
    192.168.1.1:443
    192.168.1.1:80
    192.168.1.2:443
    192.168.1.2:80
```    
