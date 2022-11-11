using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TodolistScheduleService.Jobs
{
	public class DumbJob : IJob
	{
		public string JobSays { private get; set; }
		public float FloatValue { private get; set; }

		public async Task Execute(IJobExecutionContext context)
		{
			JobKey key = context.JobDetail.Key;

			JobDataMap dataMap = context.MergedJobDataMap;  // Note the difference from the previous example

			IList<DateTimeOffset> state = (IList<DateTimeOffset>)dataMap["myStateData"];
			state.Add(DateTimeOffset.UtcNow);

			await Console.Error.WriteLineAsync("Instance " + key + " of DumbJob says: " + JobSays + ", and val is: " + FloatValue);
		}
	}
}
