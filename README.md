# Elasticsearch Dynamic Search Api
---

Bu servis sayesinde Elasticsearch üzerinde karmaşık sorgular(bool query) çalıştırabiliriz. 
 
 ## Request Body
 
 ```
 {
  "isAnd": true,
  "queryItems": [
    {
      "fieldName": "string",
      "compareOperand": 0
    }
  ],
  "queryStructures": [
    null
  ]
}
 ```
Querystructures yapısı kendi içerisine queryItems nesneleri alabilir. 

---

### Enum Compare Operands
``` cs
    Equal = 0,
    GreaterThen = 1,
    LessThen = 2,
    GreaterOrEqual = 3,
    LessOrEqual = 4,
    NotEqual = 5,
    StartsWith = 6,
    EndsWith = 7,
    Contains = 8
```
---
