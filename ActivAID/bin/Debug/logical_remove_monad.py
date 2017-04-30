import sys
#sys.path.append("C:\\Users\\Matthew\\Desktop\\191B\\model_for_trigram_output\\site-packages")
sys.path.append("C:\\Users\\Matthew\\Desktop\\191B\\model_for_trigram_output\\Lib")
from nltk.util import ngrams as get_ngrams
from nltk import word_tokenize as tokenize
import re

#filter out the indices that are in non essential words
#return both sequence with removed words and the indices to put back in
class LogicalRemoveMonad:
    n = 3 #probably keep n below 8, because the returned 6-grams will be permuted to get a distribution
    __tokens_to_remove = ["is", "are", "was", "were", "be", "being", "been",
                           "this", "that", "a", "an", "the", "an", "a",
                           "and", "or", "but","to","of","am","it","in","will",
                           "with","without","for","to","too","so","have",
                           "has","had"] 
    
    def __init__(self, *args):
        if len(args) == 1:
            self.logically_removed = LogicalRemoveMonad.__logical_remove(args[0])
        else:
            self.logically_removed = args
            
    def __or__(self, f): #bind or pipe
        check_func = f(self.logically_removed[0])
        check_func = check_func if not callable(check_func) else check_func(self.logically_removed[1])
        lrm = check_func.logically_removed
        if not lrm[1] or type(lrm[1][0]) == tuple:
            return LogicalRemoveMonad(lrm[0], self.logically_removed[1])

        return LogicalRemoveMonad(lrm[0], [tup for i, tup in enumerate(self.logically_removed[1])\
                                           if i not in lrm[1]])

    @staticmethod
    def unit(*args):
        return LogicalRemoveMonad(*args)

    @staticmethod
    def __logical_remove(text):#use stemmer and lemmatizer at probability part
        removed_list = []
        tok_list = []
        for ngram in get_ngrams(tokenize(text), LogicalRemoveMonad.n):
            #if tok in LogicalRemoveMonad.__tokens_to_remove:
            accum = tuple()
            remove = []
            for i, tok in enumerate(ngram):
                if tok in LogicalRemoveMonad.__tokens_to_remove:
                    remove.append((tok,i))
                    continue
                accum = accum + (tok,)
            removed_list.append(tuple(remove))
            tok_list.append(accum)
        return tok_list, removed_list
                
def interpolate_removed_words(ngrams):
    def nest(interp_tups):
        phrase_list = []
        for ngram, interp in zip(ngrams, interp_tups):
            phrase_list.append(__interp(ngram, interp))

        return LogicalRemoveMonad(phrase_list,[])
    return nest

def __interp(ngram, interp):
    phrase = list(ngram)
    for word, index in interp:
        phrase.insert(index, word)
    return tuple(phrase)

def resolve_keywords_only_keyword_tups(keywords):
    def only_keyword_tups(ngrams):
        ztup = []
        omitted =[] 
        for i, ngram in enumerate(ngrams):
            if any(word in keywords for word in ngram):
                ztup.append(ngram)
            else:
                omitted.append(i)
        return LogicalRemoveMonad(ztup, omitted)
    return only_keyword_tups

def remove_useless_tups(ngrams):
    ztup = []
    omitted = []
    for i, ngram in enumerate(ngrams):
        if not ngram or ngram == ("",):
            omitted.append(i)
        else:
            ztup.append(ngram)
            
    return LogicalRemoveMonad(ztup, omitted)

def remove_punctuation(ngrams):
    ztup = []
    for ngram in ngrams:
        new = tuple()
        for word in ngram:
            new = new+(re.sub(",|\\.|\\?|\\||&|:|;|!","",word),)
        ztup.append(new)
    return LogicalRemoveMonad(ztup, [])

def to_lower(ngrams):
    ztup = []
    for ngram in ngrams:
        new = tuple()
        for word in ngram:
            new = new + (word.lower(),)
        ztup.append(new)
    return LogicalRemoveMonad(ztup, [])

def clean_up_empty_strings(ngrams):
    ztup = []
    for j, ngram in enumerate(ngrams):
        new = tuple()
        for n in ngram:
            if n:
                new = new + (n,)
        ztup.append(new)

    return LogicalRemoveMonad(ztup, [])
