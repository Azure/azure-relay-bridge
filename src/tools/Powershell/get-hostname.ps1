<#
.SYNOPSIS
Gets the hostnames (and corresponding IP addresses) specified in the hosts file.

.DESCRIPTION
The hosts file is used to map hostnames to IP addresses.

.EXAMPLE
.\Get-HostNames.ps1

 IpAddress     Hostname                                                   
 ---------     --------                                                    
 127.0.0.1     localhost 
 127.0.0.1     zurich-ac-portal-test.dnsdemo1.com    # Added by HybridConnectionClient
#>

begin
{
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"

    function CreateHostsEntryObject(
        [string] $ipAddress,
        [string[]] $hostnames,
        <# [string] #> $comment) #HACK: never $null if type is specified
    {
        $hostsEntry = New-Object PSObject
        $hostsEntry | Add-Member NoteProperty -Name "IpAddress" `
            -Value $ipAddress

        [System.Collections.ArrayList] $hostnamesList =
            New-Object System.Collections.ArrayList

        $hostsEntry | Add-Member NoteProperty -Name "Hostnames" `
            -Value $hostnamesList

        If ($hostnames -ne $null)
        {
            $hostnames | foreach {
                $hostsEntry.Hostnames.Add($_) | Out-Null
            }
        }

        $hostsEntry | Add-Member NoteProperty -Name "Comment" -Value $comment

        return $hostsEntry
    }

    function ParseHostsEntry(
        [string] $line)
    {
        $hostsEntry = CreateHostsEntryObject

        Write-Debug "Parsing hosts entry: $line"

        If ($line.Contains("#") -eq $true)
        {
            If ($line -eq "#")
            {
                $hostsEntry.Comment = [string]::Empty
            }
            Else
            {
                $hostsEntry.Comment = $line.Substring($line.IndexOf("#") + 1)
            }

            $line = $line.Substring(0, $line.IndexOf("#"))
        }

        $line = $line.Trim()

        If ($line.Length -gt 0)
        {
            $hostsEntry.IpAddress = ($line -Split "\s+")[0]

            Write-Debug "Parsed address: $($hostsEntry.IpAddress)"

            [string[]] $parsedHostnames = $line.Substring(
                $hostsEntry.IpAddress.Length + 1).Trim() -Split "\s+"

            Write-Debug ("Parsed hostnames ($($parsedHostnames.Length)):" `
                + " $parsedHostnames")

            $parsedHostnames | foreach {
                $hostsEntry.Hostnames.Add($_) | Out-Null
            }
        }

        return $hostsEntry
    }

    function ParseHostsFile
    {
        $hostsEntries = New-Object System.Collections.ArrayList

        [string] $hostsFile = $env:WINDIR + "\System32\drivers\etc\hosts"

        If ((Test-Path $hostsFile) -eq $false)
        {
            Write-Verbose "Hosts file does not exist."
        }
        Else
        {
            [string[]] $hostsContent = Get-Content $hostsFile

            $hostsContent | foreach {
                $hostsEntry = ParseHostsEntry $_

                $hostsEntries.Add($hostsEntry) | Out-Null
            }
        }

        # HACK: Return an array (containing the ArrayList) to avoid issue with
        # PowerShell returning $null (when hosts file does not exist)
        return ,$hostsEntries
    }

    [Collections.ArrayList] $hostsEntries = ParseHostsFile
}

process
{
    $hostsEntries | foreach {
        $hostsEntry = $_

        $hostsEntry.Hostnames | foreach {
            $properties = @{
                Hostname = $_
                IpAddress = $hostsEntry.IpAddress
				Comment = $hostsEntry.Comment
            }

            New-Object PSObject -Property $properties
        }
    }
}