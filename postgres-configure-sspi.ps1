# Requires PowerShell 6+
# Run in your PostgreSQL data directory
# The pg_ctl reload command may require running it with Administrator rights
write-host 'Start powershell script'
$env:Path += ";c:\program files\PostgreSQL\13\bin"

$currentLocation = Get-location -Path
write-host 'Path was: ' $currentLocation
Set-location -Path 'c:\program files\PostgreSQL\13\data\'

# Add a user mapping for domain users (or local users if your computer is not in a domain) to your pg_ident.conf
(Get-Content -Path pg_ident.conf) `
  -replace "# MAPNAME       SYSTEM-USERNAME         PG-USERNAME",`
           "# MAPNAME       SYSTEM-USERNAME         PG-USERNAME`r`nDomainOrLocalUser       /^(.*)@$env:USERDOMAIN         \1" | `
  Set-Content -Path pg_ident.conf

# Enable SSPI for the current user in your pg_hba.conf and reference the user mapping
(Get-Content -Path pg_hba.conf ) `
  -replace "# `"local`" is for Unix domain socket connections only",`
           "# `"local`" is for Unix domain socket connections only`r`nlocal   all             $env:USERNAME                                    sspi map=DomainOrLocalUser" `
  -replace "# IPv4 local connections:",`
           "# IPv4 local connections:`r`nhost    all             $env:USERNAME            127.0.0.1/32            sspi map=DomainOrLocalUser" `
  -replace "# IPv6 local connections:",`
           "# IPv6 local connections:`r`nhost    all             $env:USERNAME            ::1/128                 sspi map=DomainOrLocalUser" | `
  Set-Content -Path pg_hba.conf

# The following commands will fail if the PostgreSQL bin directory is not in your PATH
# Reload the configuration so that the changes above are in effect
pg_ctl reload -D .

$env:PGPASSWORD='Password12!';

# Create a database user for the current Windows user who is a cluster admin with all rights
createuser -dilrs --replication -U postgres

Set-location -Path $currentLocation
# Create an own database for the current Windows user for initial connection
#createdb --maintenance-db=postgres
# After this has successfully run, you should be able to connect to your cluster just by typing psql

