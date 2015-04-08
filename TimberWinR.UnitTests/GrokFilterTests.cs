﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using TimberWinR.Parser;
using Newtonsoft.Json.Linq;

namespace TimberWinR.UnitTests
{
    [TestFixture]
    public class GrokFilterTests
    {      
        [Test]
        public void TestMatch()
        {
            JObject json = new JObject
            {
                {"LogFilename", @"C:\\Logs1\\test1.log"},
                {"Index", 7},
                {"Text", null},
                {"type", "Win32-FileLog"},
                {"ComputerName", "dev.mycompany.net"}
            };

            string grokJson = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""type"": ""Win32-FileLog"",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""add_tag"":[  
                            ""rn_%{Index}"",
                            ""bar""
                        ],
		                ""add_field"":[  
			                ""host"",
			                ""%{ComputerName}""
		                ]
		            }
	                }]
                }
            }";

            Configuration c = Configuration.FromString(grokJson);

            Grok grok = c.Filters.First() as Grok;

            Assert.IsTrue(grok.Apply(json));

            // Verify host field added
            Assert.AreEqual(json["host"].ToString(), "dev.mycompany.net");

            // Verify two tags added
            Assert.AreEqual(json["tags"][0].ToString(), "rn_7");
            Assert.AreEqual(json["tags"][1].ToString(), "bar");
        }

        [Test]
        public void TestRemoveFields()
        {
            JObject json = new JObject
            {
                {"LogFilename", @"C:\\Logs1\\test1.log"},
                {"Index", 7},
                {"Text", "crap"},
                {"type", "Win32-FileLog"},
                {"ComputerName", "dev.mycompany.net"}
            };

            string grokJson = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""\""[type]\"" == \""Win32-FileLog\"""",
		                ""match"":[  
			                ""Text"",
			                ""crap""
		                ],
                        ""remove_field"":[  
                            ""Index"",
                            ""LogFilename""
                        ]		               
		            }
	                }]
                }
            }";

            Configuration c = Configuration.FromString(grokJson);

            Grok grok = c.Filters.First() as Grok;

            Assert.IsTrue(grok.Apply(json));          

            // Verify index removed
            Assert.IsNull(json["Index"]);

            // Verify index removed
            Assert.IsNull(json["LogFilename"]);        
        }

        [Test]
        public void TestDropConditions()
        {
            JObject json1 = new JObject
            {
                {"LogFilename", @"C:\\Logs1\\test1.log"},
                {"EventType", 1},
                {"Index", 7},
                {"Text", null},
                {
                    "tags", new JArray
                    {
                        "tag1",
                        "tag2"
                    }
                },
                {"type", "Win32-FileLog"},
                {"ComputerName", "dev.mycompany.net"}
            };

            JObject json2 = new JObject
            {
                {"LogFilename", @"C:\\Logs1\\test1.log"},
                {"EventType", 2},
                {"Index", 7},
                {"Text", null},
                {
                    "tags", new JArray
                    {
                        "tag1",
                        "tag2"
                    }
                },
                {"type", "Win32-FileLog"},
                {"ComputerName", "dev.mycompany.net"}
            };


            string grokJsonDropIf1 = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{                          
		                ""condition"": ""[EventType] == 1"",
                        ""drop"": ""true"",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_tag"":[  
                            ""tag1""                          
                        ]		               
		            }
	                }]
                }
            }";

            string grokJsonDropIfNot1 = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{                          
		                ""condition"": ""[EventType] != 1"",
                        ""drop"": ""true"",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_tag"":[  
//                            ""tag1""                          
                        ]		               
		            }
	                }]
                }
            }";


            // Positive Tests
            Configuration c = Configuration.FromString(grokJsonDropIf1);
            Grok grok = c.Filters.First() as Grok;
            Assert.IsFalse(grok.Apply(json1));

            c = Configuration.FromString(grokJsonDropIf1);
            grok = c.Filters.First() as Grok;
            Assert.IsTrue(grok.Apply(json2));

            // Negative Tests
            c = Configuration.FromString(grokJsonDropIfNot1);
            grok = c.Filters.First() as Grok;
            Assert.IsFalse(grok.Apply(json2));

            c = Configuration.FromString(grokJsonDropIfNot1);
            grok = c.Filters.First() as Grok;
            Assert.IsTrue(grok.Apply(json1));




        }



        [Test]
        public void TestConditions()
        {
            JObject json = new JObject
            {
                {"LogFilename", @"C:\\Logs1\\test1.log"},
                {"Index", 7},
                {"Text", null},
                {"tags", new JArray
                    {
                        "tag1",
                        "tag2"
                    }
                },
                {"type", "Win32-FileLog"},
                {"Type", "Win32-MyType"},
                {"ComputerName", "dev.mycompany.net"}
            };

            string grokJson1 = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""\""[type]\"" == \""Win32-FileLog\"""",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_tag"":[  
                            ""tag1""                          
                        ]		               
		            }
	                }]
                }
            }";

            string grokJson2 = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""\""[type]\"".Contains(\""Win32-FileLog\"")"",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_tag"":[  
                            ""tag1""                          
                        ]		               
		            }
	                }]
                }
            }";


            string grokJson3 = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""\""[type]\"".Contains(\""Win32-Filelog\"")"",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_tag"":[  
                            ""tag1""                          
                        ]		               
		            }
	                }]
                }
            }";

            string grokJson4 = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""!\""[Type]\"".StartsWith(\""[\"") && !\""[Type]\"".EndsWith(\""]\"") && (\""[type]\"" == \""Win32-FileLog\"")"",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_tag"":[  
                            ""tag1""                          
                        ]		               
		            }
	                }]
                }
            }";

            
            Configuration c = Configuration.FromString(grokJson4);
            Grok grok = c.Filters.First() as Grok;
            Assert.IsTrue(grok.Apply(json));


            // Positive Tests
            c = Configuration.FromString(grokJson1);
            grok = c.Filters.First() as Grok;
            Assert.IsTrue(grok.Apply(json));

            c = Configuration.FromString(grokJson2);
            grok = c.Filters.First() as Grok;
            Assert.IsTrue(grok.Apply(json));

            // Negative Test
            c = Configuration.FromString(grokJson3);
            grok = c.Filters.First() as Grok;
            Assert.IsTrue(grok.Apply(json));
        }

        [Test]
        public void TestRemoveTags()
        {
            JObject json = new JObject
            {
                {"LogFilename", @"C:\\Logs1\\test1.log"},
                {"Index", 7},
                {"Text", null},
                {"tags", new JArray
                    {
                        "tag1",
                        "tag2"
                    }
                },
                {"type", "Win32-FileLog"},
                {"ComputerName", "dev.mycompany.net"}
            };

            string grokJson = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""\""[type]\"" == \""Win32-FileLog\"""",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_tag"":[  
                            ""tag1""                          
                        ]		               
		            }
	                }]
                }
            }";

            Configuration c = Configuration.FromString(grokJson);

            Grok grok = c.Filters.First() as Grok;

            Assert.IsTrue(grok.Apply(json));

            Assert.IsTrue(json["tags"].Children().Count() == 1);            
            Assert.AreEqual(json["tags"][0].ToString(), "tag2");
        }

        [Test]
        public void TestMatchCount()
        {
            string grokJson = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""\""[type]\"" == \""Win32-FileLog\"""",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_tag"":[  
                            ""tag1""                          
                        ]		               
		            }
	                }]
                }
            }";

            Configuration c = Configuration.FromString(grokJson);
            Grok grok = c.Filters.First() as Grok;
            Assert.AreEqual(2, grok.Match.Length);
        }

        [Test]
        public void TestRemoveFieldCount()
        {
            string grokJson = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""\""[type]\"" == \""Win32-FileLog\"""",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_field"":[  
                            ""ComputerName""                          
                        ]		               
		            }
	                }]
                }
            }";

            Configuration c = Configuration.FromString(grokJson);
            Grok grok = c.Filters.First() as Grok;
            Assert.IsTrue(grok.RemoveField.Length >= 1);
        }

        [Test]
        public void TestAddFieldCount()
        {
            string grokJson = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""\""[type]\"" == \""Win32-FileLog\"""",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""remove_tag"":[  
                            ""tag1""                          
                        ],
                        ""add_field"":[
                            ""host"",
                            ""%{ComputerName}""
                        ]		               
		            }
	                }]
                }
            }";

            Configuration c = Configuration.FromString(grokJson);
            Grok grok = c.Filters.First() as Grok;
            Assert.IsTrue(grok.AddField.Length % 2 == 0);
        }

        [Test]
        public void TestAddTagCount()
        {
            string grokJson = @"{  
            ""TimberWinR"":{        
                ""Filters"":[  
	                {  
		            ""grok"":{  
		                ""condition"": ""\""[type]\"" == \""Win32-FileLog\"""",
		                ""match"":[  
			                ""Text"",
			                """"
		                ],
                        ""add_tag"":[  
                            ""rn_%{RecordNumber}"",
                            ""bar""                          
                        ]		               
		            }
	                }]
                }
            }";

            Configuration c = Configuration.FromString(grokJson);
            Grok grok = c.Filters.First() as Grok;
            Assert.IsTrue(grok.AddTag.Length >= 1);
        }

    }
}
