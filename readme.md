           .__                           
    ______ |  |__ _____     ____   ____  
    \____ \|  |  \\__  \   / ___\_/ __ \ 
    |  |_> >   Y  \/ __ \_/ /_/  >  ___/ 
    |   __/|___|  (____  /\___  / \___  >
    |__|        \/     \//_____/      \/ 

**Twitter:** @phage_nz  
**GitHub:** phage-nz  
**Blog:** https://phage.nz  

https://github.com/phage-nz/autopsy 

A one-click tool wrapper for initial investigation of suspect or compromised Windows machines. Designed to be usable by **everyone**.  


## Usage ##
Run autopsy.exe as administrator and answer the questions. That's it! autopsy can also be run in offline mode, however you must first run it on a machine with internet access so that the toolset and most recent signature base can be retrieved. Once collected, copy the full folder structure (including \res) to the target and run as per usual. In 'autopsy.exe.config' is an appsettings key where you can define if the operation should be online or offline, but unless \res is populated it will not work.  

Resulting log and memory dump are produced on the desktop of the user. Analysis of these will of course still need to be performed by someone with relevant expertise.  


## Current Toolset ##
**Loki** (https://github.com/Neo23x0/Loki)  
**DumpIt** (https://blog.comae.io/your-favorite-memory-toolkit-is-back-f97072d33d5c) - ready to throw straight into Volatility (https://github.com/volatilityfoundation/volatility)
