# Grok Filter
The grok filter allows you to parse unstructured data into something structured and 
queryable.  The very same patterns used by logstash are supported in TimberWinR.  
See [Grok Patterns](https://github.com/elasticsearch/logstash/blob/v1.4.2/patterns/grok-patterns)

## Grok Basics

Grok works by combining text patterns into something that matches your logs.

The syntax for a grok pattern is %{SYNTAX:SEMANTIC} 

The SYNTAX is the name of the pattern that will match your text. For example, �3.44� will be matched by the NUMBER pattern and �55.3.244.1� 
will be matched by the IP pattern. The syntax is how you match. 

The SEMANTIC is the identifier you give to the piece of text being matched. For example, �3.44� could be the duration of an event, so you could 
call it simply �duration�. Further, a string �55.3.244.1� might identify the �client� 
making a request.  

For the above example, your grok filter would look something like this:

%{NUMBER:duration} %{IP:client}


## Grok Operations
The following operations are allowed when mutating a field.

| Operation       |     Type        | Description                                                            
| :---------------|:----------------|:-----------------------------------------------------------------------|
| *add_field*     | property:array  |If the filter is successful, add an arbitrary field to this event.  Field names can be dynamic and include parts of the event using the %{field} syntax.  This property must be specified in pairs.                                    
| *add_tag*       | property:array  |If the filter is successful, add an arbitrary tag to this event.  Tag names can be dynamic and include parts of the event using the %{field} syntax.                                  
| *condition*     | property:string |C# expression
| *match*         | property:array  |Required field must match (any) before any subsequent grok operations are executed.
| *remove_field*  | property:array  |If the filter is successful, remove arbitrary fields from this event.  Field names can be dynamic and include parts of the event using the %{field} syntax.                                
| *remove_tag*    | property:array  |If the filter is successful, remove arbitrary tags from this event.  Field names can be dynamic and include parts of the event using the %{field} syntax.                          
| *rename*        | property:array  |Rename one or more fields                                       
| *type*          | property:string |Type to which this filter applies, if empty, applies to all types.

## Operation Details
### match 
The match field is required, the first argument is the field to inspect, and compare to the expression specified by the second
argument.  In the below example, the message is spected to be something like this from a fictional sample log:

```
55.3.244.1 GET /index.html 15824 0.043
```

The pattern for this could be:
```
%{IP:client} %{WORD:method} %{URIPATHPARAM:request} %{NUMBER:bytes} %{NUMBER:duration}
```
Given this configuration
```json
  "Filters": [     
    {
	   "grok": {
           "match": [
               "message",
               "%{IP:client} %{WORD:method} %{URIPATHPARAM:request} %{NUMBER:bytes} %{NUMBER:duration}"
           ],
           "add_tag": [               
               "http_log"
           ],
           "add_field": [
               "verb", "%{method}"
           ]
        }           
    }     
  ]
```

Given this configuration
```json
  "Filters": [     
    {
	   "grok": {
           "matches": [
               "message",
               "%{IP:client} %{WORD:method} %{URIPATHPARAM:request} %{NUMBER:bytes} %{NUMBER:duration}"
           ],
           "add_tag": [               
               "http_log"
           ],
           "add_field": [
               "verb", "%{method}"
           ]
        }           
    }     
  ]
```


And if the message matches, then 6 fields would be added to the event:
 1. client=55.3.244.1
 2. method=GET
 3. request=/index.html
 4. bytes=15824
 5. duration=0.043
 6. verb=GET

And the following tag will be added
 1. tag: { "http_log" }

### condition "C# expression"
If present, the condition must evaluate to true in order for the remaining operations to be performed.  If there is no condition specified
then the operation(s) will be executed in order.
```json
  "Filters": [     
    {
		"grok": {      
          "type": "Win32-EventLog",		  
			"add_field": [
				"ComputerName", "%{Host}"				              
			]
		}                
    }     
  ]
```
The above example will add a field ComputerName set to the value of Host only for Win32-EventLog types.

### add_field ["fieldName", "fieldValue", ...]
The fields must be in pairs with fieldName first and value second.
```json
  "Filters": [     
    {
		"grok": {      			
			"add_field": [
              "ComputerName", "Host",
              "Username", "%{SID}"				         
			]
		}                
    }     
  ]
```

### remove_field ["tag1", "tag2", ...]
Remove the fields.  More than one field can be specified at a time.
```json
  "Filters": [     
    {
		"grok": {      			
			"remove_tag": [             
             "static_tag1",
             "Computer_%{Host}"
			]
		}                
    }     
  ]
```


### add_tag ["tag1", "tag2", ...]
Adds the tag(s) to the tag array.
```json
  "Filters": [     
    {
		"grok": {      			
			"add_tag": [
               "foo_%{Host}",
			   "static_tag1"      
			]
		}                
    }     
  ]
```

### remove_tag ["tag1", "tag2", ...]
Remove the tag(s) to the tag array.  More than one tag can be specified at a time.
```json
  "Filters": [     
    {
		"grok": {      			
			"remove_tag": [             
             "static_tag1",
             "Username"
			]
		}                
    }     
  ]
```
