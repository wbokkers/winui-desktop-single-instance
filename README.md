# WinUI Desktop Single Instance Example

Example on how to create a single instance WinUI Desktop app.

Try to start this app multiple times (e.g. from the start menu) to see that only one app instance keeps running.

This app can also be used as a CLI and processes command line arguments. To make this work, an execution alias is added to the manifest (see the manifest of the package project for details).

For example type this on the command prompt:

```
> SingleInstanceExample argument1 argument2
```

And see the arguments displayed in the main window:

![](CLI-result-example.png)


## How to use this in your app?

Add ```SingleInstanceDesktopApp.cs``` to your app and use this class from ```App.Xaml.cs```.
Create an instance of SingleInstanceDesktop app in the constructor of the App class, and attach an event handler to the Launched event.

You need a unique ID for your app in the constructor of SingleInstanceDesktopApp. You can use a GUID string.

In the normal OnLaunched method, call the Launch method of the SingleInstanceDesktopApp instance.

In the attached event handler, you will receive SingleInstanceLaunchEventArgs with 2 properties:

* IsFirstLaunch  
  A flag indicating if this is the first app launch. Only create the UI (main window) on the first launch. 

* Arguments  
  A string containing the command line arguments. 
  When processing the arguments, don't assume that you are running on the UI thread. So use the DispatcherQueue when updating UI.


If you want your app to run from the command line, make sure to add this extension to your package manifest:

  ``` xml
   <Extensions>
        <uap3:Extension
              Category="windows.appExecutionAlias"
              EntryPoint="Windows.FullTrustApplication">
          <uap3:AppExecutionAlias>
            <desktop:ExecutionAlias Alias="MyAlias.exe" />
          </uap3:AppExecutionAlias>
        </uap3:Extension>
   </Extensions>
```
