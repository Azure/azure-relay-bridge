<#
.SYNOPSIS
Adds one or more hostnames to the hosts file.

.DESCRIPTION
The hosts file is used to map hostnames to IP addresses.

.PARAMETER IPAddress
The IP address to map the hostname(s) to.

.PARAMETER Hostnames
One or more hostnames to map to the specified IP address.

.PARAMETER Comment
Optional comment that is written above the new hosts entry.

.EXAMPLE
.\Add-Hostnames.ps1 127.0.0.1 foobar

Description
-----------
Adds the following line to the hosts file (assuming "foobar" does not already
exist in the hosts file):

127.0.0.1    foobar

A warning is displayed if "foobar" already exists in the hosts file and is
mapped to the specified IP address. An error occurs if "foobar" is already
mapped to a different IP address.

.EXAMPLE
.\Add-Hostnames.ps1 127.0.0.1 foo, bar "This is a comment"

Description
-----------
Adds the following lines to the hosts file (assuming "foo" and "bar" do not
already exist in the hosts file):

# This is a comment
127.0.0.1    foo bar

A warning is displayed if either "foo" or "bar" already exists in the hosts
file and is mapped to the specified IP address. An error occurs if "foo" or
"bar" is already mapped to a different IP address.

.NOTES
This script must be run with administrator privileges.
#>
param(
    [parameter(Mandatory = $true)]
    [string] $IPAddress,
    [parameter(Mandatory = $true, ValueFromPipeline = $true)]
    [string[]] $Hostnames,
    [string] $Comment
)

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

    function UpdateHostsFile(
        $hostsEntries = $(Throw "Value cannot be null: hostsEntries"))
    {
        Write-Verbose "Updatings hosts file..."

        [string] $hostsFile = $env:WINDIR + "\System32\drivers\etc\hosts"

        $buffer = New-Object System.Text.StringBuilder

        $hostsEntries | foreach {

            If ([string]::IsNullOrEmpty($_.IpAddress) -eq $false)
            {
                $buffer.Append($_.IpAddress) | Out-Null
                $buffer.Append("`t") | Out-Null
            }

            If ($_.Hostnames -ne $null)
            {
                [bool] $firstHostname = $true

                $_.Hostnames | foreach {
                    If ($firstHostname -eq $false)
                    {
                        $buffer.Append(" ") | Out-Null
                    }
                    Else
                    {
                        $firstHostname = $false
                    }

                    $buffer.Append($_) | Out-Null
                }
            }

            If ($_.Comment -ne $null)
            {
                If ([string]::IsNullOrEmpty($_.IpAddress) -eq $false)
                {
                    $buffer.Append(" ") | Out-Null
                }

                $buffer.Append("#") | Out-Null
                $buffer.Append($_.Comment) | Out-Null
            }

            $buffer.Append([System.Environment]::NewLine) | Out-Null
        }

        [string] $hostsContent = $buffer.ToString()

        $hostsContent = $hostsContent.Trim()

        Set-Content -Path $hostsFile -Value $hostsContent -Force -Encoding ASCII

        Write-Verbose "Successfully updated hosts file."
    }

    [bool] $isInputFromPipeline =
        ($PSBoundParameters.ContainsKey("Hostnames") -eq $false)

    [int] $pendingUpdates = 0

    [Collections.ArrayList] $hostsEntries = ParseHostsFile
}

process
{
    If ($isInputFromPipeline -eq $true)
    {
        $items = $_
    }
    Else
    {
        $items = $Hostnames
    }

    $newHostsEntry = CreateHostsEntryObject $IpAddress
    $hostsEntries.Add($newHostsEntry) | Out-Null

    $items | foreach {
        [string] $hostname = $_

        [bool] $isHostnameInHostsEntries = $false

        for ([int] $i = 0; $i -lt $hostsEntries.Count; $i++)
        {
            $hostsEntry = $hostsEntries[$i]

            Write-Debug "Hosts entry: $hostsEntry"

            If ($hostsEntry.Hostnames.Count -eq 0)
            {
                continue
            }

            for ([int] $j = 0; $j -lt $hostsEntry.Hostnames.Count; $j++)
            {
                [string] $parsedHostname = $hostsEntry.Hostnames[$j]

                Write-Debug ("Comparing specified hostname" `
                    + " ($hostname) to existing hostname" `
                    + " ($parsedHostname)...")

                If ([string]::Compare($hostname, $parsedHostname, $true) -eq 0)
                {
                    $isHostnameInHostsEntries = $true

                    If ($ipAddress -ne $hostsEntry.IpAddress)
                    {
                        Throw "The hosts file already contains the" `
                            + " specified hostname ($parsedHostname) and it is" `
                            + " mapped to a different address" `
                            + " ($($hostsEntry.IpAddress))."
                    }

                    Write-Verbose ("The hosts file already contains the" `
                        + " specified hostname ($($hostsEntry.IpAddress) $parsedHostname).")
                }
            }
        }

        If ($isHostnameInHostsEntries -eq $false)
        {
            Write-Debug ("Adding hostname ($hostname) to hosts entry...")

            $newHostsEntry.Hostnames.Add($hostname) | Out-Null
            $pendingUpdates++
        }
    }
}

end
{
    If ($pendingUpdates -eq 0)
    {
        Write-Verbose "No changes to the hosts file are necessary."

        return
    }

    Write-Verbose ("There are $pendingUpdates pending update(s) to the hosts" `
        + " file.")

    UpdateHostsFile $hostsEntries
}