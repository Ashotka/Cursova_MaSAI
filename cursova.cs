using System; 
using System.Collections.Generic; 
using System.Globalization; 
using System.ComponentModel; 
using System.Data; 
using System.Drawing; 
using System.Text; 
using System.Windows.Forms; 
using System.Data.Sql; 
using System.Data.SqlClient; 
using kurs_fcm.Core; 
 
namespace kurs_fcm 
{ 
    public partial class frmMain : Form 
    { 
        int maxValue = 100; 
        List<CustomerObject> clusters = new List<CustomerObject>(); 
        List<CustomerObject> points = new List<CustomerObject>(); 
        double[,] mMatrix; 
        double[,] minimax; 
        int counter = 0; 
        List<string> inColumns = new List<string>(); 
        DataSet ds = new DataSet(); 
        public frmMain() 
        { 
            InitializeComponent(); 
            RemoveTabs(); 
            edtClusterNumber.Paste(trackBar1.Value.ToString()); 
            double n = 1+trackBar2.Value/20.0; 
            edtM.Paste(n.ToString()); 
            n = trackBar3.Value; 
            edtIter.Paste(n.ToString()); 
            pictureBox1.Focus(); 
        } 
        private void RemoveTabs() 
        { 
            tabControl1.TabPages.Remove(tabPage2); 
            tabControl1.TabPages.Remove(tabPage3); 
            tabControl1.TabPages.Remove(tabPage4); 
        } 
        private void AddTab(TabPage tp) 
        { 
            tabControl1.TabPages.Add(tp); 
        } 
        private void button1_Click(object sender, EventArgs e) 
        { 
            try 
            { 
                button1.Enabled = false; 
                Update(); 
                string con = @"Persist Security Info=False;Integrated Security=SSPI; 
                           database=AdventureWorksDW;server=(local)"; 
                SqlConnection cn = new SqlConnection(con);
SqlCommand cmd = new SqlCommand(SelectQuery.query, cn); 
                SqlDataAdapter da = new SqlDataAdapter(cmd); 
         
                da.Fill(ds); 
                dtvCustomer.DataSource = ds.Tables[0]; 
                dtvCustomer.AutoGenerateColumns = true; 
                dtvCustomer.Refresh(); 
                toolStripLabel1.Text += dtvCustomer.RowCount.ToString()+ " записів"; 
                BindValuesTab(ds); 
                AddTab(tabPage2); 
            } 
            catch 
            { 
                toolStripLabel1.Text += "Помилка"; 
                button1.Enabled = true; 
            } 
        } 
        private void BindValuesTab(DataSet ds) 
        { 
            DataTable dsn = new DataTable(); 
            DataColumn dcHeader = new DataColumn("Header"); 
            dsn.Columns.Add(dcHeader); 
            DataColumn dcValue = new DataColumn("Value"); 
            dcValue.DataType = System.Type.GetType("System.Boolean"); 
            dsn.Columns.Add(dcValue); 
 
            int row = ds.Tables[0].Columns.Count; 
            DataRow dr; 
            for (int count = 1; count < row; count++) 
            { 
                bool isNum = true; 
                string tp = ds.Tables[0].Columns[count].DataType.ToString(); 
                if ((tp == "System.Char") | (tp == "System.Boolean") | (tp == 
"System.DateTime") 
                    | (tp == "System.String") | (tp == "System.TimeSpan")) isNum = 
false; 
                if (isNum) 
                { 
                    dr = dsn.NewRow(); 
                    dr["Header"] = ds.Tables[0].Columns[count].ColumnName; 
                    dr["Value"] = false; 
                    dsn.Rows.Add(dr); 
                } 
            } 
            dtvCases.DataSource = dsn; 
            dtvCases.Refresh(); 
        } 
 
        private void dtvCases_CurrentCellDirtyStateChanged(object sender, EventArgs e) 
        { 
            if (dtvCases.IsCurrentCellDirty) 
            { 
                dtvCases.CommitEdit(DataGridViewDataErrorContexts.Commit); 
            } 
        } 
 
        private void dtvCases_CellBeginEdit(object sender, 
DataGridViewCellCancelEventArgs e) 
        { 
            if (e.ColumnIndex != 1) e.Cancel = true; 
        }
private void trackBar1_ValueChanged(object sender, EventArgs e) 
        { 
            edtClusterNumber.Text=(trackBar1.Value.ToString()); 
        } 
 
        private void trackBar2_ValueChanged(object sender, EventArgs e) 
        { 
            double n =  1+(trackBar2.Value/20.0 ); 
            edtM.Text=(n.ToString()); 
        } 
 
        private void trackBar3_ValueChanged(object sender, EventArgs e) 
        { 
            double n = trackBar3.Value; 
            edtIter.Text=(n.ToString()); 
        } 
 
        private void dtvCases_CellValueChanged(object sender, DataGridViewCellEventArgse) 
        { 
            bool needAdd = false; 
            if ((bool)dtvCases.CurrentCell.Value == true) 
            { 
                counter++; 
                needAdd = true; 
            } 
            else 
            { 
                counter--; 
                needAdd = false; 
            } 
            toolStripLabel2.Text = "Вибрано: " + (counter) + " характеристик"; 
            if (dtvCases.Columns[e.ColumnIndex].Name == "Value") 
            { 
                string str = dtvCases[e.ColumnIndex-1,e.RowIndex].Value.ToString(); 
                if (inColumns.Contains(str) & (needAdd == false)) 
                    inColumns.Remove(str); 
                else inColumns.Add(str); 
            } 
        } 
 
        private void button2_Click(object sender, EventArgs e) 
        { 
            try 
            { 
                points.Clear(); 
                clusters.Clear(); 
                
                int col = ds.Tables[0].Columns.Count; 
                int row = ds.Tables[0].Rows.Count; 
 
                List<double> inpValues = new List<double>(); 
                minimax = new double[inColumns.Count, 2]; 
                int ind = 0; 
                for (int i = 1; i < col; i++) 
                { 
                    string name = ds.Tables[0].Columns[i].ColumnName; 
                    if (inColumns.Contains(name)) 
                    { 
                        double val = Convert.ToDouble(ds.Tables[0].Rows[0][name], 
CultureInfo.InvariantCulture);
minimax[ind, 0] = val; 
                        minimax[ind, 1] = val; 
                        ind++; 
                    } 
                } 
                for (int r = 1; r < row; r++) 
                { 
                    int index = 0; 
                    for (int count = 1; count < col; count++) 
                    { 
                        string name = ds.Tables[0].Columns[count].ColumnName; 
                        if (inColumns.Contains(name)) 
                        { 
                            try 
                            { 
                                double val = 
Convert.ToDouble(ds.Tables[0].Rows[r][name], CultureInfo.InvariantCulture); 
                                if (val <= minimax[index, 0]) 
                                    minimax[index, 0] = val; 
                                if (val >= minimax[index, 1]) 
                                    minimax[index, 1] = val; 
                                index++; 
                            } 
                            catch 
                            { 
                                MessageBox.Show("Cannot convert a value"); 
                            } 
                        } 
                    } 
 
                } 
                for (int r = 0; r < row; r++) 
                { 
                    for (int count = 1; count < col; count++) 
                    { 
                        string name = ds.Tables[0].Columns[count].ColumnName; 
                        if (inColumns.Contains(name)) 
                        { 
                            try 
                            { 
                                double val = 
Convert.ToDouble(ds.Tables[0].Rows[r][name], CultureInfo.InvariantCulture); 
                                inpValues.Add(val); 
                            } 
                            catch 
                            { 
                                MessageBox.Show("Cannot convert a value"); 
                            } 
                        } 
                    } 
                    int id = Convert.ToInt32(ds.Tables[0].Rows[r][0]); 
                    AddPoints(inpValues, id); 
                    inpValues.Clear(); 
                } 
                RunClustering(); 
                button3.Enabled = true; 
            }
catch (Exception ex) 
            { 
                MessageBox.Show(ex.Message); 
            } 
        } 
        private void AddPoints(List<double> inputList, int id) 
        { 
            double[] a = inputList.ToArray(); 
            for (int i = 0; i < a.Length; i++) 
                a[i] = NormalizeValue(a[i],minimax[i,0],minimax[i,1]); 
            CustomerObject cst = new CustomerObject(a,id); 
            points.Add(cst); 
        } 
        private double NormalizeValue(double inp, double min,double max) 
        { 
            double intervLength = max-min; 
            double nV = maxValue*(inp-min)/intervLength; 
            return nV; 
        } 
        private void RunClustering() 
        { 
            try 
            { 
                PrepareClusters(); 
                double m = Convert.ToDouble(edtM.Text, CultureInfo.InvariantCulture); 
                int maxIter = Convert.ToInt32(edtIter.Text); 
                CoreAlgorithm alg = new CoreAlgorithm(points, clusters, m, maxIter); 
                alg.StartClustering(); 
                mMatrix = alg.solMatrix; 
            } 
            catch 
            { 
                MessageBox.Show("Помилка при роботі алгоритму"); 
            } 
        } 
        private void PrepareClusters() 
        { 
            List<int> usedNumbers = new List<int>(); 
            Random random = new Random(); 
            int row = ds.Tables[0].Rows.Count; 
            int cltCount = Convert.ToInt32(edtClusterNumber.Text); 
            int val = 0; 
            for (int i = 0; i < cltCount; i++) 
            { 
                do 
                { 
                    val = random.Next(0, row); 
                } while (usedNumbers.Contains(val)); 
                clusters.Add(points[val]); 
            } 
        } 
        private void button3_Click(object sender, EventArgs e) 
        { 
            if (!tabControl1.Contains(tabPage3)) 
                AddTab(tabPage3); 
            tabControl1.SelectedTab = tabPage3; 
            PrepateResults(); 
        } 
        private void PrepateResults() 
        {
comboBox1.Items.Clear(); 
            int n = Convert.ToInt32(edtClusterNumber.Text); 
            edtClust.Text = edtClusterNumber.Text; 
            for (int i = 0; i < n; i++) 
                comboBox1.Items.Add("Кластер "+i.ToString()); 
            comboBox1.Focus(); 
         
        } 
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) 
        { 
            BindResults(); 
        } 
 
        private void BindResults() 
        { 
             
            int n = comboBox1.SelectedIndex; 
            int r = 0; 
            DataTable rt = ds.Tables[0].Clone(); 
            rt.Columns.Add("Probability", typeof(double)); 
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++) 
            { 
                if (Math.Truncate(points[i].ClusterNumber) == n) 
                { 
                    rt.ImportRow(ds.Tables[0].Rows[i]); 
                    for (int j = 0; j<clusters.Count ;j++ ) 
                        if (j==n) 
                        rt.Rows[r]["Probability"] = mMatrix[i, j]; 
                    r++; 
                } 
            } 
            dtvResults.DataSource = rt; 
            dtvResults.Refresh(); 
            edtCount.Text = rt.Rows.Count.ToString(); 
        } 
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) 
        { 
            if (tabControl1.SelectedTab == tabPage3) 
            { 
                comboBox1.SelectedItem = -1; 
                try 
                { 
                    BindResults(); 
                } 
                catch { } 
            } 
 
        } 
    } 
} 
 
using System; 
using System.Collections.Generic; 
using System.Text; 
 
namespace kurs_fcm.Core 
{ 
    // Represent the main fuzzy logic clustering algorithm 
 
    public class CoreAlgorithm 
    {
public int MaxIter = 15; 
        // All customers 
        private List<CustomerObject> _customersObjects; 
         
        // All clusters 
        private List<CustomerObject> _clusters; 
 
        // Matrix of mamebership values 
        private double [,] uMatrix; 
 
        // M-factor 
        private double _m=2.0; 
 
        // Eps-factor 
        private double _epsilon=0.01; 
 
        // Eps-factor 
        private double _precision=Math.Pow(10,-4); 
 
        // Objective function 
        private double J; 
         
        // Alg iterations 
        public int Iter; 
        // Properties 
 
        public double[,] solMatrix 
        { 
            get { return uMatrix;} 
        } 
 
        public double M 
        { 
            get { return _m; } 
        } 
 
        public double Eps 
        { 
            get { return _epsilon; } 
        } 
 
        public double Precision 
        { 
            get { return _precision; } 
        } 
 
        // Constructor 
        public CoreAlgorithm(List<CustomerObject> customers, List<CustomerObject> 
newClusters) 
        { 
            this._customersObjects = customers; 
            this._clusters = newClusters; 
            InitializeUMatrix(_customersObjects, _clusters); 
            UpdateClusters(_customersObjects, _clusters); 
        } 
 
        // Reload constructor 
        public CoreAlgorithm(List<CustomerObject> customers, List<CustomerObject> 
newClusters, double newM, int maxIter) 
        { 
            this._customersObjects = customers; 
            this._clusters = newClusters; 
            this._m = newM; 
            this.MaxIter = maxIter; 
            InitializeUMatrix(_customersObjects, _clusters);  
 
 
            UpdateClusters(_customersObjects, _clusters); 
        } 
 
        // Minkowski metric 
        private double FindDistance(CustomerObject First, CustomerObject Second) 
        { 
            if (First.Dimension!=Second.Dimension) throw new Exception("Error in 
dimensions"); 
            double dist = 0.0; 
            for (int i = 0; i < First.Dimension; i++) 
            { 
                dist+= Math.Pow(First.Objects[i]-Second.Objects[i],2); 
            } 
            dist = Math.Sqrt(dist); 
            return dist; 
        } 
         
        // Find value of objective function  
        private double ObjFunctVal() 
        { 
            double Funct_Val = 0; 
            for (int i = 0; i < _customersObjects.Count; i++) 
            { 
                for (int j = 0; j < _clusters.Count; j++) 
                { 
                    Funct_Val += Math.Pow(uMatrix[i, j], M) * 
Math.Pow(FindDistance(_customersObjects[i], _clusters[j]), 2); 
                } 
            } 
            return Funct_Val; 
        } 
        // Calculate new cluster index for each object 
        private void UpdateClusters(List<CustomerObject> customers, List<CustomerObject> 
clusters) 
        { 
            for (int i = 0; i < customers.Count; i++) 
            { 
                double prob = -1.0; 
                var cust = customers[i]; 
                for (int j = 0; j < clusters.Count; j++) 
                { 
                    if (prob < uMatrix[i, j]) 
                    { 
                        prob = uMatrix[i, j]; 
                        if (prob == 0.5) cust.ClusterNumber = 0.5; 
                        else cust.ClusterNumber = j; 
                    } 
                } 
            } 
        } 
         
        // Update centroid values 
        private void UpdateCentroids() 
        { 
            for (int j = 0; j < _clusters.Count; j++) 
            { 
                CustomerObject clt = _clusters[j]; 
                double[] mCust = new double[clt.Dimension]; 
                double summ = 0.0; 
 
                for (int i = 0; i < _customersObjects.Count; i++) 
                { 
                    CustomerObject cst = _customersObjects[i]; 
                    double powMatrix = Math.Pow(uMatrix[i, j], M);  
 
 
                    for (int k = 0; k < cst.Dimension; k++) 
                    { 
                        mCust[k] += powMatrix * cst.Objects[k]; 
                    } 
                    summ += powMatrix; 
                } 
                for (int h = 0; h < clt.Dimension; h++) 
                { 
                    clt.Objects[h] = (mCust[h] / summ); 
                } 
            } 
        } 
 
        private bool InitializeUMatrix(List<CustomerObject> customers, List<CustomerObject> 
clusters) 
        { 
            bool Res = false; 
            double dist; 
            try 
            { 
                uMatrix = new double [customers.Count,clusters.Count ]; 
                double firstSumma = 0.0; 
                double secondSumma = 0.0; 
                CustomerObject obj; 
                CustomerObject clt; 
                for (int i = 0; i < customers.Count; i++) 
                { 
                    obj = customers[i]; 
                    firstSumma = 0.0; 
                    for (int j = 0; j < clusters.Count; j++) 
                    { 
                        clt = clusters[j]; 
                        dist = FindDistance(obj, clt); 
                        if (dist == 0) uMatrix[i, j] = Eps; 
                        else uMatrix[i, j] = dist; 
                        firstSumma += uMatrix[i, j]; 
                    } 
                    secondSumma = 0.0; 
                    for (int j = 0; j < clusters.Count; j++) 
                    { 
                        uMatrix[i, j] = 1.0 / Math.Pow(uMatrix[i, j] / firstSumma, 2.0 / (M - 1.0)); 
                        secondSumma += uMatrix[i, j]; 
                    } 
 
                    for (int j = 0; j < clusters.Count; j++) 
                    { 
                        uMatrix[i, j] /= secondSumma; 
                    } 
                } 
                Res = true; 
            } 
            catch 
            { 
                 
            } 
            return Res; 
        } 
 
        // Iteration 
        private void NextIteration() 
        { 
            for (int i = 0; i < _clusters.Count; i++) 
            { 
                for (int jj = 0; jj < _customersObjects.Count; jj++)  
                { 
                    double dist = FindDistance(_customersObjects[jj], _clusters[i]); 
                    if (dist < 1.0) dist = Eps; 
                    double total = 0.0; 
                    for (int k = 0; k < _clusters.Count; k++) 
                    { 
                        double newDist = FindDistance(_customersObjects[jj], _clusters[k]); 
                        if (newDist < 1.0) newDist = Eps; 
                        total += Math.Pow(dist / newDist, 2.0 / (M - 1.0)); 
                    } 
                    uMatrix[jj, i] = (double)(1.0 / total); 
                } 
            } 
            UpdateClusters(_customersObjects,_clusters); 
        } 
 
        public void StartClustering() 
        { 
            double newJ; 
            int i = 0; 
            do 
            { 
                i++; 
                J = ObjFunctVal(); 
                UpdateCentroids(); 
                NextIteration(); 
                newJ = ObjFunctVal(); 
                if (Math.Abs(J - newJ) < Precision) break; 
            } 
            while (MaxIter > i); 
            Iter = i; 
        } 
    } 
} 
using System; 
using System.Collections.Generic; 
using System.Text; 
 
namespace kurs_fcm.Core 
{ 
    public static class SelectQuery 
    { 
        public static string query =  
    @" 
       Select  
            f.CustomerKey, 
          c.FirstName, 
          c.LastName, 
          c.BirthDate, 
          c.Gender, 
          c.YearlyIncome, 
          c.TotalChildren,  
          c.NumberChildrenAtHome as ChildrenAtHome, 
          c.NumberCarsOwned, 
          c.DateFirstPurchase , 
           sum(f.SalesAmount) as SalesAmount 
       From FactInternetSales f INNER JOIN DimCustomer c ON  
          f.CustomerKey=c.CustomerKey 
       Group By f.CustomerKey,c.FirstName, 
          c.LastName, 
          c.BirthDate, 
          c.Gender, 
          c.YearlyIncome, 
          c.TotalChildren,  
          c.NumberChildrenAtHome,  
 
 
          c.NumberCarsOwned, 
          c.DateFirstPurchase 
       Order by f.CustomerKey"; 
    } 
} 
 
using System; 
using System.Collections.Generic; 
 
namespace kurs_fcm.Core 
{ 
    // Represent test object  
    public class CustomerObject 
    { 
        //Id of each point 
        int id; 
        // An index of cluster  
        // It can be double (reason: using fuzzy logic) 
        private double _cluster; 
        
        // Cluster coordinates in n-dimensions  
        private List<double> _objects; 
        // Dimension value 
         
        private int _dimension; 
         
        // Safe property of cluster index 
        public double ClusterNumber 
        { 
            get { return _cluster;} 
            set { _cluster = value; } 
        } 
 
        // Safe proeprty of cluster coordinates 
        public List<double> Objects 
        { 
            get { return _objects; } 
            set { _objects = value; } 
        } 
 
        // Safe property of dimension value 
        public int Dimension 
        { 
            get { return Objects.Count; } 
            set { _dimension = value; } 
        } 
 
        // Public constructor 
        public CustomerObject() 
        { 
            Objects = new List<double>(); 
            ClusterNumber = -1.0; 
        } 
        public CustomerObject(List<double> newCoordinates) 
        { 
            Objects = new List<double>(); 
            Objects = newCoordinates; 
            ClusterNumber = -1.0; 
        } 
 
        public CustomerObject(double[] coord,int inpId) 
        { 
            Objects = new List<double>(); 
            for (int i = 0; i < coord.Length; i++) 
                Objects.Add(coord[i]);  
            ClusterNumber = -1.0; 
            id = inpId; 
        } 
 
        public bool SetCoordinates(List<double> newCoordinates) 
        { 
            bool Res=false; 
            try 
            { 
                Objects = newCoordinates; 
                Res = true; 
            } 
            catch 
            { 
                throw new Exception("Cannot set new values"); 
            } 
            return Res; 
        } 
         
    } 
}
