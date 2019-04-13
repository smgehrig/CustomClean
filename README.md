# CustomClean

### about  

Custom Clean is a Visual Studio extension for cleaning custom build locations.  

If there are many build output locations or customizations in MSBuild, it may become difficult to clean all artifacts with a generic clean.  
This tool will allow you to delete the contents of defined directories. It also allows the tool to ignore certain file names and file types during the deletion.  

### modifying or installing  

To make customizations clone the repo and build. Please open a PR if you would like your changes to be added.   

To install: when the project is built, run the VSIX file generated. This will install the extension into Visual Studio.  


### Configuring the extension  

An xml file must named CustomClean.xml must be placed into the root directory of the project. (next to the sln)   

<details><summary>Example</summary>
<p>
  
  nameexceptions will ignore files with the specified names  
  filetypeexceptions will ignore files with the specified file type
  
```xml
<SETTINGS>
	<GENERALSETTINGS>
		<STANDARDCLEAN>true</STANDARDCLEAN>
	</GENERALSETTINGS>
	<DIRECTORIES>
		<PATH>BuildOutput</PATH>
	</DIRECTORIES>
	<IGNORE>
		<NAMEEXCEPTIONS>
			<NAME>test</NAME>
		</NAMEEXCEPTIONS>
		<FILETYPEEXCEPTIONS>
			<TYPE>.txt</TYPE>
		</FILETYPEEXCEPTIONS>
	</IGNORE>
</SETTINGS>
```  
</p>
</details>  

### Using the extension  

After the extension is installed, a new option will be placed under the 'Build' menu of the toolbar.   
![toolbar](_media/toolbar.png)  



