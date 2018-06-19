<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
  <xsl:output method="xml" indent="yes" />
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
  <xsl:key name="azbridgesvc-srch" match="wix:Component[substring(wix:File/@Source, string-length(wix:File/@Source) - string-length('azbridge.exe') +1)='azbridge.exe']" use="@Id" />
  <xsl:template match="wix:Component[key('azbridgesvc-srch', @Id)]">
    <xsl:copy>
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates select="*|text()" />
      <wix:ServiceInstall Id="InstallABS"
                  Name="azbridgesvc"
                  Description="Azure Relay Bridge Service"
                  Start="auto"
                  ErrorControl="normal"
                  Type="ownProcess" 
                  Arguments="--svc"   />
      <wix:ServiceControl Id="ControllABS"
              Name="azbridgesvc"
              Start="install"
              Stop="both"
              Remove="uninstall"
              Wait="yes" />
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>