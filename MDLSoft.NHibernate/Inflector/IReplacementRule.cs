namespace MDLSoft.NHibernate.Inflector
{
	public interface IReplacementRule : IRuleApplier
	{
		string Replacement { get; }
		string Pattern { get; }
	}
}