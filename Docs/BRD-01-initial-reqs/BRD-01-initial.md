# This file has my basic initial requirements to start with.

1. I want to create a angular .net core app (name = GenericScreenGenApp) for creating angular front end screens driven by screen config json file.
2. Each Screen will have a separate json file. The name of json file will be the same as Screen name (convert '-' into space, exclude first prefic of screen-). The naming convention will be screen-name of-my-screen.json.
3. Each json file should have below format
3.1 Section (mandatory) - with a name and layout-policy; defaults are name=default-section; layout-policy=per-line
3.2 List of Fields within a section
3.3 Section can have nested sections
3.4 Sections should be collapsible
3.5 Each section should be defined  with a border on UI
3.6 layout-policy=per-line should display every field on 1 line
3.7 No need to draw a border for default-section
3.8 Each field will have these attributes --> id/name/description/type/width.
3.9 Field description should be used as context sensitive help when cursor hovers over the field
4. All the screen config files will be stored in screen folder
5. The dynamic screen generator will read the screen json files from folder and present a menu to the user for selecting a screen.
6. Once the screen is selected, the app will dynamically create the simple screen layout with sections and fields as defined in screen config file.
7. Create a sample Calculator screen config file which shows below fields - file name = Screen-Calculator.json
7.1 Section = default
7.2 Field #1 : id=1/name=First Number/description=Type first number to add/Type=integer/width=300.px
7.3 Field #2 : id=2/name=Second Number/description=Type second number to add/Type=integer/width=300.px
7.4 Field #3 : id=3/name=Calculate/description=Add these 2 numbers/Type=button/width=`00.px
