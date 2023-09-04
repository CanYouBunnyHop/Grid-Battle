using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LinqLearn : MonoBehaviour
{
    public List<LinqObject> objs = new();
    
    public List<int> objs2 = new();
    
    void Start()
    {
        objs = new()
        {
            new LinqObject(1),
            new LinqObject(2),
            new LinqObject(3),
            new LinqObject(1),
            new LinqObject(1),
            new LinqObject(2)
        };

        var query = objs.GroupBy(x => x.id).Where(g => g.Count() > 1).Select(y => y.Key);
        objs2 = query.ToList();
        // var query = lst.GroupBy(x => x)
        //   .Where(g => g.Count() > 1)
        //   .Select(y => y.Key)
        //   .ToList();
    }
}
[System.Serializable]
public class LinqObject
{
    public int id;
    public string name;
    public LinqObject(int _id)
    {
        id = _id;
    }
}
