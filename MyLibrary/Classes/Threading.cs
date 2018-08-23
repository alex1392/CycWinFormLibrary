using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary.Classes
{
  /// <summary>
  /// 背景工作隊列物件，可動態加入背景工作並使工作依序執行
  /// </summary>
  public class TaskQueue
  {
    public Queue<Task> queue;
    public TaskQueue()
    {
      queue = new Queue<Task>();
    }

    /// <summary>
    /// 加入新的工作到隊列中
    /// </summary>
    /// <param name="task">新工作</param>
    public void Enqueue(Task task)
    {
      task.ContinueWith(_ => { queue.Dequeue(); }); //工作完成後移出隊列
      if (queue.Count > 0)
      {
        queue.Last().ContinueWith(_ => { task.Start(); }); //鏈接前一個工作
      }
      
      queue.Enqueue(task);
      if (queue.Count == 1)
      {
        task.Start(); //第一個工作
      }
    }

    /// <summary>
    /// 加入新的工作到隊列中
    /// </summary>
    /// <param name="action">要執行的動作</param>
    public void Enqueue(Action action)
    {
      var task = new Task(action);
      Enqueue(task);
    }

  }

  public class BackgroundArgs
  {
    public object sender;
    public EventArgs e;
    public object[] parameters;
    public object[] results;

    public BackgroundArgs(object sender)
    {
      this.sender = sender;
    }
    public BackgroundArgs(EventArgs e, object[] parameters)
    {
      this.e = e;
      this.parameters = parameters;
    }
    public BackgroundArgs(object sender, EventArgs e)
    {
      this.sender = sender;
      this.e = e;
    }
    public BackgroundArgs(object sender, EventArgs e, object[] parameters)
    {
      this.sender = sender;
      this.e = e;
      this.parameters = parameters;
    }
  }
}
