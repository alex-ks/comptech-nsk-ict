import sys
import os
import numpy

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
    for i in range(len(values)):
        results.append(float(values[i]))
    
    result = numpy.mean(results)    
    
        
    
    with open(os.path.join(sys.argv[1],str(1)+"output.txt"), 'w') as f:
        f.write(str(result))
    print(1)