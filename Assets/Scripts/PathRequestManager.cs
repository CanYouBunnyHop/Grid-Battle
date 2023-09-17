using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathReqQueue = new Queue<PathRequest>();
    PathRequest curPathReq;
    public static PathRequestManager thePathReqManager;
    public PathFind pathFind;
    [SerializeField]bool isProcessing = false;
    public bool IsProcessing => isProcessing;
    [SerializeField]int stepsAfterFinishProcessing;
    private void Awake()
    {
        //isProcessing = false;
        thePathReqManager = this;
        pathFind = GetComponent<PathFind>();
    }
    public void RequestPathFindings(Cell _start, List<Cell> _end, bool _isAirbourne, bool _dash, Action<Cell[], bool> _callback)
    {
        PathRequest newReq = new PathRequest(_start, _end, _isAirbourne, _dash, _callback);
        thePathReqManager.pathReqQueue.Enqueue(newReq);

        thePathReqManager.TryProcessNext();
    }
    void TryProcessNext()
    {
        if(!isProcessing && pathReqQueue.Count > 0)
        {
            curPathReq = pathReqQueue.Dequeue();
            IEnumerator c = pathFind.StartFindPath(curPathReq.pathStart, curPathReq.targets, curPathReq.isAirbourne, curPathReq.isDash);
            StartCoroutine(c);
            isProcessing = true;
        }
    }
    public void FinishedProcessingPath(Cell[] _path, bool _success)
    {
        try
        {
            isProcessing = false;
            curPathReq.callback(_path, _success);
            TryProcessNext();
        }
        catch(NullReferenceException){Debug.Log("Finish Null Error");}
    }
    struct PathRequest
    {
        public Cell pathStart;
        public List<Cell> targets;
        public bool isAirbourne;
        public bool isDash;
        public Action<Cell[], bool> callback;

        public PathRequest(Cell _start, List<Cell> _targets, bool _isAirbourne, bool _isDash, Action<Cell[], bool> _callback)
        {
            pathStart = _start;
            targets = _targets;
            isAirbourne = _isAirbourne;
            isDash = _isDash;
            callback = _callback;
        }
    }
    private void FixedUpdate()
    {
        if(isProcessing) stepsAfterFinishProcessing = 0;
        else stepsAfterFinishProcessing +=1;
    }
    public bool AlreadyFinishedProcessing() => !isProcessing && stepsAfterFinishProcessing > 3;
}
