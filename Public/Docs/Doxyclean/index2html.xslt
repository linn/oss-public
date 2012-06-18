<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:date="http://exslt.org/dates-and-times" version="1.0" extension-element-prefixes="date">
	<xsl:output method="html" omit-xml-declaration="yes" indent="yes" />
	
	<xsl:template match="/">
		<xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
		<html>
		<head>
			<meta charset="UTF-8" />
			<title><xsl:apply-templates select="project" mode="title"/></title>
			
			<meta id="Generator" name="Generator" content="Doxyclean"/>
			<meta id="GeneratorVersion" name="GeneratorVersion" content="2.2"/>
			
			<link rel="stylesheet" type="text/css" href="css/common.css"/>
			<link rel="stylesheet" type="text/css" media="screen" href="css/screen.css"/>
			<link rel="stylesheet" type="text/css" media="print" href="css/print.css"/>
		</head>
		<body>
		    <header id="projectHeader">
				<h1><xsl:apply-templates select="project" mode="title"/></h1>
			</header>
			<header id="fileHeader">
				<h1><a href="#classTitle"><xsl:apply-templates select="object" mode="title"/></a></h1>
			</header>
			<div id="indexContainer">
				
				<xsl:if test="count(project/object[@kind='class']) > 0">
					<div class="column">
						<h5>Class References</h5>
						<ul>
							<xsl:apply-templates select="project/object[@kind='class']">
								<xsl:sort select="normalize-space(name)"/>
							</xsl:apply-templates>
						</ul>
					</div>
				</xsl:if>
				
				<div class="column">
					<xsl:if test="count(project/object[@kind='protocol']) > 0">
						<h5>Protocol References</h5>
						<ul>
							<xsl:apply-templates select="project/object[@kind='protocol']">
							    <xsl:sort select="normalize-space(name)"/>
							</xsl:apply-templates>
						</ul>
					</xsl:if>
						
					<xsl:if test="count(project/object[@kind='category']) > 0">
						<h5>Category References</h5>
						<ul>
							<xsl:apply-templates select="project/object[@kind='category']">
								<xsl:sort select="normalize-space(name)"/>
							</xsl:apply-templates>
						</ul>
					</xsl:if>

					<xsl:if test="count(project/object[@kind='interface']) > 0">
						<h5>Interface References</h5>
						<ul>
							<xsl:apply-templates select="project/object[@kind='interface']">
								<xsl:sort select="normalize-space(name)"/>
							</xsl:apply-templates>
						</ul>
					</xsl:if>
				</div>
				
				<div style="clear:left;"/>
				<hr style="margin-top:15px;margin-bottom:15px;"/>
				<p id="lastUpdated" style="margin-top:4px;margin-bottom:11px;height:17px;line-height:17px;">Last updated: <xsl:value-of select="date:year()"/>-<xsl:value-of select="date:month-in-year()"/>-<xsl:value-of select="date:day-in-month()"/></p>
			
			</div>
			
		</body>
		</html>
	</xsl:template>
	
	<xsl:template match="project" mode="title">
		<xsl:if test="@name">
			<xsl:value-of select="@name"/> 
		</xsl:if>
		Project Reference
	</xsl:template>
	
	<xsl:template match="object">
		<li>
			<a>
				<xsl:attribute name="href">
					<xsl:choose>
						<xsl:when test="@kind='class'">Classes/</xsl:when>
						<xsl:when test="@kind='category'">Categories/</xsl:when>
						<xsl:when test="@kind='protocol'">Protocols/</xsl:when>
                        <xsl:when test="@kind='interface'">Interfaces/</xsl:when>
					</xsl:choose>
					<xsl:choose><xsl:when test="string-length(substring-before(normalize-space(name), '::')) > 0"><xsl:value-of select="substring-before(normalize-space(name), '::')"/>_</xsl:when><xsl:otherwise><xsl:value-of select="normalize-space(name)"/></xsl:otherwise></xsl:choose><xsl:choose><xsl:when test="string-length(substring-after(substring-after(normalize-space(name), '::'), '::')) > 0"><xsl:value-of select="substring-before(substring-after(normalize-space(name), '::'), '::')"/>_<xsl:value-of select="substring-after(substring-after(normalize-space(name), '::'), '::')"/></xsl:when><xsl:otherwise><xsl:value-of select="substring-after(normalize-space(name), '::')"/></xsl:otherwise></xsl:choose>.html</xsl:attribute><xsl:value-of select="normalize-space(name)"/>
			</a>
		</li>
	</xsl:template>
	
</xsl:stylesheet>
