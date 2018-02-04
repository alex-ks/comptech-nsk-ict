import sys
import os




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
    
    nameFiles = os.listdir(sys.argv[1]) 
    nameFiles.remove("source.py")
    
    for i in range(len(nameFiles)):
        with open(os.path.join(sys.argv[1],nameFiles[i]), 'r') as f:
            arg = f.readline()
        with open(os.path.join(sys.argv[1],str(i)+"output.txt"), 'w') as f:
            f.write(arg)
       
    print(1)