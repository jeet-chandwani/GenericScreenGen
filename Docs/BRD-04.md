# This file has my requirements for adding fine layout policies and field types.

**Req 4.1.** Layout and field type changes as listed below:
    **Req 4.1.0**. We need to add a max character or max width policy to all field types. This will be used to determine how much space a field should take up in the layout, even when field is empty or screen width is more than needed. This will be used to determine how much space a field should take up in the layout, even when field is empty or screen width is more than needed.
    **Req 4.1.1**. Each screen should have an id and a name. Th ename coulkd defaulty to file name without the prefix "Screen-" and suffix ".json". This will be used to identify the screen in the layout and to display the screen name in the UI.
    **Req 4.1.2**. We need to centralize the features for a screen with attribute. Example - functionality of Save , cancel and Show Original values can be centralized with attribute and then we can use that attribute in the screen to enable or disable those features. This will help us to avoid code duplication and make it easier to maintain the code.
    **Req 4.1.3**. We need to add handling of mandatory fields in the layout only if save functionality is enabled for the screen that has the section layout. This will help us to ensure that the user fills in all the required fields before saving the data.

**Req 4.2** Screen level changes as follows:
    **Req 4.2.1**. Each screen should have a screen id and screen name. Name should be derived from config file name excluding prefix "Screen-" and suffix ".json" as default. The user can manually change the name as needed. This will be used to identify the screen in the layout and to display the screen name in the UI.
    **Req 4.2.2**. We need to centralize the features for a screen with attribute. Example - functionality of Save , cancel and Show Original values can be centralized with attribute and then we can use that attribute in the screen to enable or disable those features. This will help us to avoid code duplication and make it easier to maintain the code.
    **Req 4.2.3**. Need to mark id/name/features and dections as mandatory fields in the screen JSON schema. This will help us to ensure that the user fills in all the required fields before saving the data. 
    **Req 4.2.4**. For a given section, at least 1 field should be mandatory. This will help us to ensure that the user fills in at least one field in the section before saving the data. These attributes arer also mandaotry for each section --> name, layout-policy and is-collapsible.
    **Req 4.2.5**. For a given field object, these attributes are mandatory --> id,name,descriotion, type and width. This will help us to ensure that the user fills in all the required fields before saving the data and also to ensure that the field is rendered correctly in the UI.

