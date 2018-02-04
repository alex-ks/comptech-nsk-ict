import sys
import os
import scipy.stats as stats

def isInt(s):
    try:
        int(s)
        return True
    except ValueError:
        return False
    

if __name__ == "__main__":
    
    
    argFiles  = []
    nameFiles = []
    values = [] 
    result = 0
    results = []
    nameFiles = os.listdir(sys.argv[1])
    nameFiles.remove("source.py") 
    with open(os.path.join(sys.argv[1],nameFiles[0]), 'r') as f:
        arg = f.readline()
    
    values = arg.split(' ')
    if (len(values) >= 20):
        for i in range(len(values)):
            results.append(float(values[i]))
        
        statist,hi_2 = stats.normaltest(results)
        with open(os.path.join(sys.argv[1],str(1)+"output.txt"), 'w') as f:
            f.write(str(hi_2))
        print(1)
    else:
        print(0)