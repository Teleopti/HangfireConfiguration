# Requires PowerShell 6+
# Run in your PostgreSQL data directory
# The pg_ctl reload command may require running it with Administrator rights
$password = $args[0]
if($password -eq $null)
{
	$password =  Read-Host 'Give the password for the user postgres on your postgreSql database'
}

$env:Path += ";c:\program files\PostgreSQL\13\bin"

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
Write-Host 'Password: ' $password

$env:PGPASSWORD=$password;

# Create a database user for the current Windows user who is a cluster admin with all rights
createuser -dilrs --replication -U postgres
