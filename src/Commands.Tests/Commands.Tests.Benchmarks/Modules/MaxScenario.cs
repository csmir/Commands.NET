namespace Commands.Tests
{
    public sealed class MaxScenario : ModuleBase
    {
        public class MaxScenarioFormattable
        {
            public override string ToString()
            {
                return Guid.NewGuid().ToString();
            }
        }

        [Name("scenario-exception")]
        public Exception ExceptionResult()
        {
            return new Exception("This is an exception.");
        }

        [Name("scenario-task-exception")]
        public Task<Exception> ExceptionTaskResult()
        {
            return Task.FromResult(new Exception("This is an exception."));
        }

        [Name("scenario-exception-throw")]
        public void ExceptionResultThrow()
        {
            throw new Exception("This is an exception.");
        }

        [Name("scenario-task-exception-throw")]
        public async Task ExceptionTaskResultThrow()
        {
            await Task.Delay(1);
            throw new Exception("This is an exception.");
        }

        [Name("scenario-operation-mutation")]
        public int OperationMutation()
        {
            int i = 0;
            while (i < 100)
            {
                i++;
            }

            return i;
        }

        [Name("scenario-task-operation-mutation")]
        public Task<int> OperationTaskMutation()
        {
            int i = 0;
            while (i < 100)
            {
                i++;
            }

            return Task.FromResult(i);
        }

        [Name("scenario-operation-formattable")]
        public MaxScenarioFormattable OperationFormattable()
        {
            return new MaxScenarioFormattable();
        }

        [Name("scenario-task-operation-formattable")]
        public Task<MaxScenarioFormattable> OperationTaskFormattable()
        {
            return Task.FromResult(new MaxScenarioFormattable());
        }
    }
}
