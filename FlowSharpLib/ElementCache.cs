/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

// ElementCache is no longer necessary as the element won't be disposed, since it's part of the undo/redo action.

/*
using System.Collections.Generic;

namespace FlowSharpLib
{
    public class ElementCache
    {
        /// <summary>
        /// Used for caching deleting elements as part of undo/redo, so the element's pens, brushes, etc., are not disposed.
        /// </summary>
        protected List<GraphicElement> cachedElements = new List<GraphicElement>();
        protected static ElementCache instance;

        protected ElementCache()
        {
        }

        public static ElementCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ElementCache();
                }

                return instance;
            }
        }

        public void Add(GraphicElement el)
        {
            if (!cachedElements.Contains(el))
            {
                // Cache the element being deleted so we can do a proper dispose of deleted elements at some point.
                // TODO: When is that "point"?
                cachedElements.Add(el);
            }
        }

        public void Remove(GraphicElement el)
        {
            if (cachedElements.Contains(el))
            {
                cachedElements.Remove(el);
            }
        }

        public void ClearCache()
        {
            cachedElements.ForEach(el => el.Dispose());
            cachedElements.Clear();
        }
    }
}
*/