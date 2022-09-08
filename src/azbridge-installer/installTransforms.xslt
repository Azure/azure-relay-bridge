<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
  <xsl:output method="xml" indent="yes" />
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:key name="azbridgesvc-srch" match="wix:Component[substring(wix:File/@Source, string-length(wix:File/@Source) - string-length('azbridge.exe') +1)='azbridge.exe']" use="@Id" />
  <xsl:template match="wix:Component[key('azbridgesvc-srch', @Id)]">
    <xsl:copy>
      <xsl:attribute name="Guid">{098E424F-CABA-4217-80CC-E4C58D10AE67}</xsl:attribute>
      <xsl:attribute name="Id">ExeFile</xsl:attribute>
      <wix:Condition>NOT INSTALL_SERVICE</wix:Condition>
      <xsl:apply-templates select="*|text()">
        <xsl:with-param name="fileid">ExeFileId</xsl:with-param>
      </xsl:apply-templates>
    </xsl:copy>
    <xsl:copy>
      <xsl:attribute name="Guid">{4DD98183-3A32-4F6C-9008-77EE0ED0718A}</xsl:attribute>
      <xsl:attribute name="Id">SvcFile</xsl:attribute>
      <wix:Condition>INSTALL_SERVICE</wix:Condition>
      <xsl:apply-templates select="*|text()">
        <xsl:with-param name="fileid">SvcFileId</xsl:with-param>
      </xsl:apply-templates>
      <wix:ServiceInstall Id="InstallABS" Name="azbridgesvc" Description="Azure Relay Bridge Service" Start="demand" ErrorControl="normal" Type="ownProcess" Arguments="--svc" Vital="yes" Account="NT AUTHORITY\NETWORKSERVICE" />
      <!--<wix:ServiceControl Id="ControllABS" Name="azbridgesvc" Start="install" Stop="both" Remove="uninstall" Wait="yes" />-->
    </xsl:copy>
  </xsl:template>

  <xsl:key name="azbridge-svc-config" match="wix:File[substring(@Source, string-length(@Source) - string-length('azbridge_config.svc.yml') +1)='azbridge_config.svc.yml']" use="@Id" />
  <xsl:key name="azbridge-machine-config" match="wix:File[substring(@Source, string-length(@Source) - string-length('azbridge_config.machine.yml') +1)='azbridge_config.machine.yml']" use="@Id" />
  <xsl:key name="azbridge-exe" match="wix:File[substring(@Source, string-length(@Source) - string-length('azbridge.exe') +1)='azbridge.exe']" use="@Id" />

  <xsl:template match="wix:Wix/wix:Fragment[1]">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <wix:DirectoryRef Id="CONFIGFOLDER">
        <xsl:apply-templates select="//wix:Component/wix:File[key('azbridge-svc-config', @Id)]|//wix:Component/wix:File[key('azbridge-machine-config', @Id)]" />
      </wix:DirectoryRef>
      <xsl:apply-templates select="*|text()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:DirectoryRef[@Id='INSTALLFOLDER']">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <xsl:apply-templates select="*[not(self::wix:Component/wix:File[key('azbridge-svc-config', @Id)] or self::wix:Component/wix:File[key('azbridge-machine-config', @Id)])]|text()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:Component/wix:File[key('azbridge-svc-config', @Id)]">
    <wix:Component>
      <xsl:copy-of select="../@*" />
      <xsl:copy>
        <xsl:copy-of select="@*" />
        <xsl:apply-templates select="*|text()" />
        <wix:Permission User="[WIX_ACCOUNT_ADMINISTRATORS]" FileAllRights="yes" />
        <wix:Permission User="[WIX_ACCOUNT_USERS]" GenericRead="yes" Read="yes" />
        <wix:Permission User="[WIX_ACCOUNT_NETWORKSERVICE]" GenericRead="yes" Read="yes" />
      </xsl:copy>
    </wix:Component>
  </xsl:template>

  <xsl:template match="wix:Component/wix:File[key('azbridge-machine-config', @Id)]">
    <wix:Component>
      <xsl:copy-of select="../@*" />
      <xsl:copy>
        <xsl:copy-of select="@*" />
        <xsl:apply-templates select="*|text()" />
        <wix:Permission User="[WIX_ACCOUNT_ADMINISTRATORS]" FileAllRights="yes" />
        <wix:Permission User="[WIX_ACCOUNT_USERS]" GenericRead="yes" Read="yes" />
        <wix:Permission User="[WIX_ACCOUNT_NETWORKSERVICE]" GenericRead="yes" Read="yes" />
      </xsl:copy>
    </wix:Component>
  </xsl:template>

  <xsl:template match="wix:File[key('azbridge-exe', @Id)]">
    <xsl:param name="fileid" />
    <xsl:copy>
      <xsl:attribute name="Id">
        <xsl:value-of select="$fileid" />
      </xsl:attribute>
      <xsl:apply-templates select="node()|@KeyPath|@Source" />
    </xsl:copy>
  </xsl:template>

  <xsl:key name="azbridgesvc-srch-any" match="//wix:Component[substring(wix:File/@Source, string-length(wix:File/@Source) - string-length('azbridge.exe') +1)='azbridge.exe']" use="@Id" />
  <xsl:template match="wix:ComponentRef[key('azbridgesvc-srch-any', @Id)]">
     <wix:ComponentRef Id="SvcFile" />
     <wix:ComponentRef Id="ExeFile" />
  </xsl:template>

</xsl:stylesheet>