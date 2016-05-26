# Windows Service Manager

Windows service manager is a .NET application, where you can manage a Windows Service. If you are developing Windows Service you have to tinker with install, uninstall scripts and starting or stopping it.

This tool is pretty basic, all you have to do is copy it to the same folder as a yourservice.exe is and modify config file like this:

*************
            <setting name="ServiceName" serializeAs="String">
                <value>yourservice</value>
            </setting>
*************

Then you can start WindowsServiceDashboard.exe where you can manage that service. I might add more features in the future.

For questions: info@mjc.si
