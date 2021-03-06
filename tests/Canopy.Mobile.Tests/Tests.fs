module Canopy.Mobile.Tests

open canopy.mobile
open System.IO
open Expecto
open System.Collections.Generic
open System.Net
open OpenQA.Selenium.Appium
open OpenQA.Selenium.Appium.Android
open OpenQA.Selenium
open System.Threading
open OpenQA.Selenium.Appium.Android.Enums
open OpenQA.Selenium.Appium.Interfaces

let downloadDemoApp () =
    let localFile = FileInfo("./temp/ApiDemos-debug.apk")
    if File.Exists localFile.FullName then
        printfn "app %s already exists" localFile.FullName
    else
        let appOnServer = "http://appium.github.io/appium/assets/ApiDemos-debug.apk"
        Directory.CreateDirectory localFile.Directory.FullName |> ignore
        use client = new WebClient()
        
        printfn "downloading %s to %s" appOnServer localFile.FullName
        client.DownloadFile(appOnServer, localFile.FullName)
        printfn "app downloaded"

    localFile.FullName

[<Tests>]
let tests =
    testList "android tests" [
        testList "session tests" [
            testCase "can get device UUID" <| fun () ->
                let deviceUIID = driver.SessionDetails.["deviceUDID"].ToString()
                Expect.isNotNull deviceUIID "deviceUIID was set"

            testCase "can get dictionary data" <| fun () ->
                let dictionary : Dictionary<string, obj> = driver.SessionDetails.["desired"] |> unbox
                Expect.equal dictionary.Count 5 "desired data is set"
        ]

        
        testList "device tests" [
            testCase "can get device time" <| fun () ->
                let time = driver.DeviceTime

                Expect.equal time.Length 28 "time has correct format"
        ]
        
        testList "app strings tests" [
            testCase "can get app strings" <| fun () ->
                let appStrings = driver.GetAppStringDictionary()
                Expect.notEqual appStrings.Count 0 "app strings are not empty"

            testCase "can get app strings using lang" <| fun () ->
                let appStrings = driver.GetAppStringDictionary("en")
                Expect.notEqual appStrings.Count 0 "app strings are not empty"
        ]
        
        testList "key press tests" [
            testCase "can press key code" <| fun () ->
                driver.PressKeyCode(AndroidKeyCode.Home)

            testCase "can press key code with meta state" <| fun () ->
                driver.PressKeyCode(AndroidKeyCode.Space, AndroidKeyMetastate.Meta_Shift_On)

            testCase "can long press key code" <| fun () ->
                driver.LongPressKeyCode(AndroidKeyCode.Space)

            testCase "can long press key code with meta state" <| fun () ->
                driver.LongPressKeyCode(AndroidKeyCode.Space, AndroidKeyMetastate.Meta_Shift_On)
        ]

        
        testList "element tests" [
            testCase "can find element by Android UI Automator" <| fun () ->
                driver.StartActivity("io.appium.android.apis", ".ApiDemos")
                let byAndroidUIAutomator = new ByAndroidUIAutomator("new UiSelector().clickable(true)")
                let element = driver.FindElementById("android:id/content").FindElement(byAndroidUIAutomator)

                Expect.isNotNull element.Text "text is set"
                Expect.isGreaterThanOrEqual (driver.FindElementById("android:id/content").FindElements(byAndroidUIAutomator).Count) 1 "selects at least 1 element"
                

            testCase "can set immediate value" <| fun () ->
                driver.StartActivity("io.appium.android.apis", ".ApiDemos")
                driver.StartActivity("io.appium.android.apis", ".view.Controls1")
                let newValue = "new value"

                let editElement = driver.FindElementByAndroidUIAutomator("resourceId(\"io.appium.android.apis:id/edit\")") :?> AndroidElement 
                editElement.SetImmediateValue(newValue)

                Expect.equal editElement.Text newValue "text was changed"

            testCase "can find element by XPath" <| fun () ->
                let byXPath = "//android.widget.TextView[contains(@text, 'Animat')]"
                Expect.isNotNull (driver.FindElementByXPath(byXPath).Text) "element was found"
                Expect.isGreaterThanOrEqual (driver.FindElementsByXPath(byXPath).Count) 1 "at least one element was found"

            testCase "can find element by XPath with canopy find" <| fun () ->
                let element = Selector.XPath "//android.widget.TextView[contains(@text, 'Animat')]" |> find
                Expect.isNotNull element.Text "text is set"
        ]
    ]

[<EntryPoint>]
let main args =
    try
        let app = downloadDemoApp()
        start app
        let result = runTestsInAssembly defaultConfig args
        quit()
        result
    with _ -> -1