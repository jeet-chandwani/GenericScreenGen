# This file has my requirements for adding a registry for field types

**Req 6.1.** We should have a registry of all field types used in screen config. (screen-field-types-registry Json file). This file should be created in screen folder and should have a list of all field types with their parameters and default values. This registry will be used by screen renderer to render fields based on their type and parameters. This will also help in maintaining consistency across screens and reusability of field types. Each field type will have a unique id, name, and a list of parameters with their default values. 

**Req 6.2.** For now, each field type id will have an unique id and name

**Req 6.3.** Each field type id will have a list of name-value pairs stating possible parameters and their default values. Example list will be different for each field type and could be as follows
	{min-width : default-value1;}   
	{max-width : default-value2;}   
	{validators : {nullcheck; positive-number; numbers-less-than-N; etc ..} };
For now, validators should be an empty array.

**Req 6.4.** When a field type is used in screen config, we can override any of the default parameters defined in registry for that field type. If a parameter is not mentioned in screen config for a field, then default value from registry will be used for that parameter.