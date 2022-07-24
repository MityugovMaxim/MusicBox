This is a simple localization tool for User Tracking Usage Description field in XCode.
It is based mostly on Apploving MAX's realization, but you can add any language localization.

Also this tool have ability to show ATT/IDFA window by itself. 

HOW TO USE:
If you use it with MAX Apploving Advertisement plugin:
1) Enable AppLoving Consent Flow (AppLoving -> Integration manager -> Enable Consent Flow)

2) ENABLE AppLoving Localization (AppLoving -> Integration manager -> Localize User Tracking Usage Description).
Put the translations from the sheet to the applovin localization for English (en), Chinese (zh-Hans), French (fr), German (de), Japanese (ja), Korean(ko), Spanish (es).
If you don’t enable AppLoving localization, this will remove corresponding localizations from UTUDLocalizationSettings.asset on build postprocess (see Note below for workaround)

3) In UTUDLocalizationSettings check Use Custom Localization option. Set up your custom translation for other languages.

NOTE: 
All the AppLoving localization magic happens in MaxPostProcessBuildiOS.MaxPostProcessPbxProject. Its postprocess order is set to (int.MaxValue). So, if you make it work a bit earlier than UTUDLocalizationPostProcessor.MaxPostProcessPbxProject, than custom localizations won’t be overwritten and you can completely DISABLE AppLoving localization and use only UTUDLocalizationSettings.asset. 
But be careful if any AppLoving MAX SDK update takes effect on postrocess and/or localization workflow.

To add not existing language you need to know it's LANGUAGE CODE for XCode localization.
You can read about it here at "Understand the Language Identifier":
https://developer.apple.com/documentation/xcode/localization/choosing_localization_regions_and_scripts
ISO 639-1 for languages codes:
https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
ISO 3166-1 for countries codes
https://en.wikipedia.org/wiki/ISO_3166-1
ISO 15924 for script codes:
https://en.wikipedia.org/wiki/ISO_15924

After getting Language Code add it to "Code" field and translation to "LocalizedText"
Save it.

On build the postprocess will add localization to the XCode project.
You can check if everything alright by navigating at XCode project to Unity-iPhone -> Resources -> YOUR_LANGUAGE_CODE.lproj

STAND ALONE ATT/IDFA WINDOW:
If your iOS Advertisement plugin does not support ATT window (for example Chinese Pangle Unity SDK), you can use this plugin to automatically configure and show ATT window at your will.

DO NOT USE THIS FEATURE IF YOUR PLUGIN ALREADY SUPPORTS 
ATT / IDFA WINDOW!

How to:
Find the ATTDialogSettings.asset (please, dont change the name!) and turn “Use Custom ATT Dialog” ON.
Fill SkAdNetwork IDs (you can find them somewhere at your Advertizer)
Fill UTUDLocalizationSettings.assets like in instruction above
In code add:
ATTDialog.CallATTrackingDialog(
                (s) => {
                    // init your Ad Plugin here
                });

This will call ATT window, callback fires when user chooses dialog option or if it is already determined.
Build project.

If any problems or questions arise, you can contact me by Slack or email:
Vladimir Kuzmin - kuzmin.v@azurgames.com

_________________

UTUDLocalizationSettings contains localization for:
1)  ru 			- Russian
2)  zh-Hant		- Taiwan Chinese traditional
3)  zh-Hans 	- Chinese in the simplified script
4)  fr 			- France
5)  de 			- German
6)  hi 			- Hindi for India
7)  id 			- Indonesian
8)  it 			- Italian
9)  ja 			- Japanese
10) ko 			- Korean
11) ms			- Malaysian
12) pl			- Polish
13) pt-br		- Portuguese for Brazil
14) pt			- Portuguese for EU
15) es			- Spanish for EU
16) es-419		- Spanish for Latin America
17) th 			- Thai
18) tr			- Turkish
19) vi			- Vietnamese
20) en			- English
